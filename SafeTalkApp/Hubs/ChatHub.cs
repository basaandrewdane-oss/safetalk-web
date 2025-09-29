using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using SafeTalkApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafeTalkApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SafeTalkAppContext db = new SafeTalkAppContext();

        // Stores connection IDs for each appointment
        private static readonly ConcurrentDictionary<string, HashSet<string>> AppointmentGroups =
    new ConcurrentDictionary<string, HashSet<string>>();
        // ----------------------
        // Connection Handling
        // ----------------------
        public override Task OnDisconnected(bool stopCalled)
        {
            foreach (var group in AppointmentGroups.Values)
                group.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        // ----------------------
        // Chat Messaging
        // ----------------------
        public void Send(string appointmentId, string message)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            var senderName = ((ClaimsIdentity)Context.User.Identity)
                .FindFirst(ClaimTypes.GivenName)?.Value ?? Context.User.Identity.Name;

            db.chat_message_tbl.Add(new ChatMessageTblModel
            {
                appointmentID = int.Parse(appointmentId),
                senderID = userId,
                message = message,
                sentAt = DateTime.Now
            });
            db.SaveChanges();

            Clients.Group($"appointment_{appointmentId}").broadcastMessage(senderName, message);
        }

        // ----------------------
        // Group Joining
        // ----------------------
        public Task JoinAppointment(string appointmentId)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID.ToString() == appointmentId);

            if (appointment == null)
            {
                throw new HubException("Appointment not found");
            }

            var appointmentStart = appointment.date.Date + appointment.startTime;
            var appointmentEnd = appointment.date.Date + appointment.endTime;

            var phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            var nowPh = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, phTimeZone);

            if (nowPh < appointmentStart)
            {
                // ❌ Too early
                Clients.Caller.notifyJoinBlocked("early", appointmentStart.ToString("f"));
                return Task.CompletedTask;
            }

            if (nowPh > appointmentEnd || appointment.status == 6)
            {
                // ❌ Already ended
                Clients.Caller.notifyJoinBlocked("ended", appointmentEnd.ToString("f"));
                return Task.CompletedTask;
            }

            // ✅ Valid join
            AppointmentGroups.AddOrUpdate(
            appointmentId,
            _ => new HashSet<string> { Context.ConnectionId },
            (_, set) =>
            {
                lock (set) // HashSet itself is not thread-safe
                {
                    set.Add(Context.ConnectionId);
                }
                return set;
            });

            return Groups.Add(Context.ConnectionId, $"appointment_{appointmentId}");
        }

        private bool IsOtherUserInRoom(string appointmentId) =>
            AppointmentGroups.TryGetValue(appointmentId, out var conns) && conns.Count > 1;

        // ----------------------
        // WebRTC Signaling
        // ----------------------
        public void SendOffer(string appointmentId, string offer) =>
            Clients.OthersInGroup($"appointment_{appointmentId}").receiveOffer(offer);

        public void SendAnswer(string appointmentId, string answer) =>
            Clients.OthersInGroup($"appointment_{appointmentId}").receiveAnswer(answer);

        public void SendIceCandidate(string appointmentId, string candidate) =>
            Clients.OthersInGroup($"appointment_{appointmentId}").receiveIceCandidate(candidate);

        // ----------------------
        // Call Requests
        // ----------------------
        public void SendCallRequest(string appointmentId, string callerName)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            if (!IsOtherUserInRoom(appointmentId))
            {
                Clients.Caller.noOtherUserInRoom();
                return;
            }

            Clients.OthersInGroup($"appointment_{appointmentId}").receiveCallRequest(callerName);
        }

        public void SendCallResponse(string appointmentId, bool accepted)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            Clients.OthersInGroup($"appointment_{appointmentId}").receiveCallResponse(accepted);
        }

        public void EndCall(string appointmentId)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            Clients.OthersInGroup($"appointment_{appointmentId}").callEnded();
        }

        // ----------------------
        // Appointment Management
        // ----------------------
        public void EndAppointment(string appointmentId)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID.ToString() == appointmentId);

            if (appointment.doctorID != userId)
                throw new HubException("Only the doctor can end the appointment early");

            appointment.status = 6; // Completed
            db.SaveChanges();

            Clients.Group($"appointment_{appointmentId}").appointmentEnded();
        }

        public void MarkCompleted(string appointmentId)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            if (!IsUserInAppointment(userId, appointmentId))
                throw new HubException("Unauthorized");

            var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID.ToString() == appointmentId);
            if (appointment == null)
                throw new HubException("Appointment not found");

            var appointmentEnd = appointment.date.Date.Add(appointment.endTime);
            var appointmentEndUtc = TimeZoneInfo.ConvertTimeToUtc(appointmentEnd, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time")); // adjust to your TZ
            if (DateTime.UtcNow < appointmentEndUtc)
                throw new HubException("Cannot complete appointment before end time");

            appointment.status = 6; // Completed
            db.SaveChanges();

            Clients.Group($"appointment_{appointmentId}").appointmentEnded();
        }

        // ----------------------
        // Helpers
        // ----------------------
        private bool IsUserInAppointment(int userId, string appointmentId)
        {
            if (!int.TryParse(appointmentId, out var id)) 
                return false;

            var appt = db.appointments_tbl.FirstOrDefault(a => a.appointmentID == id);
            if (appt == null) 
                return false;

            return appt.doctorID == userId || appt.patientID == userId;
        }

        //private bool IsAppointmentActive(AppointmentsTblModel appt)
        //{
        //    if (appt == null) 
        //        return false;

        //    // if doctor ended it early
        //    if (appt.status == 6) // ended/complete
        //        return false;

        //    var now = DateTime.Now;

        //    return now >= appt.date + appt.startTime && now <= appt.date + appt.endTime;
        //}
    }
}
