using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace SafeTalkApp.Services
{
    public class EmailService : IEmailService
    {
        public void SendEmail(EmailMessageDTO message)
        {
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(message.From ?? smtpUser),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsBodyHtml,
            };
            mailMessage.To.Add(message.To);

            smtpClient.Send(mailMessage);
        }
        public void SendVerificationEmail(string toEmail, string verificationLink)
        {
            SendEmail(new EmailMessageDTO
            {
                To = toEmail,
                Subject = "Email Verification",
                Body = $"Please verify your email by clicking the following link: <a href='{verificationLink}'>Verify Email</a>",
                IsBodyHtml = true
            });
        }
        // ✅ New Appointment Notification Email (to Doctor)
        public void SendDoctorAppointmentNotification(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = "New Appointment Request",
                Body = $"Dear Dr. {doctor.firstName} {doctor.lastName},\n\n" +
                       $"You have a new appointment request from {patient.firstName} {patient.lastName} " +
                       $"on {appointment.date:MMMM dd, yyyy} at {appointment.startTime:hh\\:mm}.\n\n" +
                       $"Please log in to confirm or manage this appointment.\n\nSafeTalk Team",
                IsBodyHtml = true
            });
        }
        // ✅ Appointment Confirmation Email (to Patient)
        public void SendPatientAppointmentConfirmation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = "Appointment Confirmation",
                Body = $"Hello {patient.firstName} {patient.lastName},\n\n" +
                       $"Your appointment request with Dr. {doctor.firstName} {doctor.lastName} " +
                       $"on {appointment.date:MMMM dd, yyyy} at {appointment.startTime:hh\\:mm} has been recorded.\n\n" +
                       $"You will be notified once the doctor confirms it.\n\nSafeTalk Team",
                IsBodyHtml = true
            });
        }
        // ✅ Appointment Approved Email (to Patient)
        public void SendPatientAppointmentApproved(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = "Appointment Confirmation",
                Body = $"Hi {patient.firstName},<br><br>" +
                       $"Your appointment with Dr. {doctor.firstName} {doctor.lastName} " +
                       $"is confirmed on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm}.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }

        // ✅ Appointment Cancelled Email (to Doctor)
        public void SendDoctorAppointmentCancellation(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = "Appointment Cancelled",
                Body = $"Dear Dr. {doctor.firstName} {doctor.lastName},<br><br>" +
                       $"The appointment with {patient.firstName} {patient.lastName} " +
                       $"on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm} has been cancelled.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }

        // ✅ Appointment Cancelled Email (to Patient)
        public void SendPatientAppointmentCancellation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = "Appointment Cancelled",
                Body = $"Hi {patient.firstName},<br><br>" +
                       $"Your appointment with Dr. {doctor.firstName} {doctor.lastName} " +
                       $"on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm} has been cancelled.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }
    }
}