using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.DTOs.Consultation;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IEmailService _emailService;

        public ConsultationService(ISafeTalkAppContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public ApiResponse<AppointmentResultDTO> GetAppointment(int appointmentID)
        {
            try
            {
                var appt = _db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentID);

                if (appt == null)
                {
                    return ApiResponse<AppointmentResultDTO>.Fail("Appointment not found");
                }
                var dto = new AppointmentResultDTO
                {
                    appointmentID = appt.appointmentID,
                    doctorID = appt.doctorID,
                    patientID = appt.patientID,
                    date = appt.date,
                    startTime = appt.startTime,
                    endTime = appt.endTime,
                };
                return ApiResponse<AppointmentResultDTO>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AppointmentResultDTO>.Fail("An error occurred while retrieving the appointment: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<ChatMessageDTO>> GetChatMessages(int appointmentID, int currentUserId)
        {
            try
            {
                var messages = (from m in _db.chat_message_tbl
                                join u in _db.user_tbl on m.senderID equals u.userID
                                where m.appointmentID == appointmentID
                                orderby m.sentAt
                                select new ChatMessageDTO
                                {
                                    messageID = m.messageID,
                                    appointmentID = m.appointmentID,
                                    senderID = m.senderID,
                                    message = m.message,
                                    sentAt = m.sentAt,
                                    senderName = u.firstName + " " + u.lastName,
                                    currentUserId = currentUserId
                                }).ToList();

                return ApiResponse<IEnumerable<ChatMessageDTO>>.Ok(messages);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ChatMessageDTO>>.Fail("An error occurred while retrieving chat messages: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<AppointmentResultDTO>> GetPatientConsultations(int userID)
        {
            try
            {
                var consultations = (from a in _db.appointments_tbl
                                     join d in _db.user_tbl on a.doctorID equals d.userID
                                     join p in _db.payment_tbl on a.appointmentID equals p.appointmentID
                                     where a.patientID == userID && p.status == PaymentStatus.Completed
                                     orderby a.date descending, a.startTime descending
                                     select new AppointmentResultDTO
                                     {
                                         appointmentID = a.appointmentID,
                                         date = a.date,
                                         startTime = a.startTime,
                                         endTime = a.endTime,
                                         status = a.status,
                                         transcriptFilePath = a.transcriptFilePath,
                                         doctorName = d.firstName + " " + d.lastName,
                                         doctorEmail = d.email,
                                         hasReferral = _db.referrals_tbl.Any(r => r.appointmentID == a.appointmentID),
                                         referralID = _db.referrals_tbl.Where(r => r.appointmentID == a.appointmentID).Select(r => r.referralID).FirstOrDefault()
                                     }).ToList();
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Ok(consultations);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Fail("An error occurred while retrieving consultations: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<AppointmentResultDTO>> GetDoctorConsultations(int userID)
        {
            try
            {
                var consultations = (from a in _db.appointments_tbl
                                     join p in _db.user_tbl on a.patientID equals p.userID
                                     join pay in _db.payment_tbl on a.appointmentID equals pay.appointmentID
                                     where a.doctorID == userID && pay.status == PaymentStatus.Completed
                                     orderby a.date descending, a.startTime descending
                                     select new AppointmentResultDTO
                                     {
                                         appointmentID = a.appointmentID,
                                         date = a.date,
                                         startTime = a.startTime,
                                         endTime = a.endTime,
                                         status = a.status,
                                         transcriptFilePath = a.transcriptFilePath,
                                         patientName = p.firstName + " " + p.lastName,
                                         patientEmail = p.email,
                                         doctorID = a.doctorID,
                                         patientID = a.patientID,
                                         hasReferral = _db.referrals_tbl.Any(r => r.appointmentID == a.appointmentID)
                                     }).ToList();
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Ok(consultations);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Fail("An error occurred while retrieving consultations: " + ex.Message);
            }
        }

        public ApiResponse<bool> CreateReferral(ReferralDTO model)
        {
            try
            {
                var referral = new ReferralTblModel
                {
                    appointmentID = model.appointmentID,
                    doctorID = model.doctorID,
                    patientID = model.patientID,
                    reason = model.reason,
                    notes = model.notes,
                    urgencyLevel = (int)model.urgencyLevel,
                    status = model.status,
                    dateCreated = DateTime.Now,
                    sentTo = model.sentTo
                };
                _db.referrals_tbl.Add(referral);
                _db.SaveChanges();

                // Send email to patient
                try
                {
                    var doctor = _db.user_tbl.Find(model.doctorID);
                    var patient = _db.user_tbl.Find(model.patientID);

                    if (doctor != null && patient != null)
                    {
                        _emailService.SendReferralCreatedEmail(patient, doctor, referral);
                    }
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine("Error sending referral email: " + emailEx.Message);
                }

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("An error occurred while creating the referral: " + ex.Message);
            }
        }

        public ApiResponse<ReferralDTO> GetReferralDetails(int referralID)
        {
            try
            {
                var referral = _db.referrals_tbl
                    .Where(r => r.referralID == referralID)
                    .Select(r => new ReferralDTO
                    {
                        appointmentID = r.appointmentID,
                        doctorID = r.doctorID,
                        patientID = r.patientID,
                        reason = r.reason,
                        notes = r.notes,
                        urgencyLevel = (UrgencyLevel)r.urgencyLevel,
                        status = r.status,
                        dateCreated = r.dateCreated,
                        sentTo = r.sentTo
                    }).FirstOrDefault();
                return ApiResponse<ReferralDTO>.Ok(referral);
            }
            catch (Exception)
            {
                return ApiResponse<ReferralDTO>.Fail("An error occurred while retrieving the referral details.");
            }
        }
    }
}