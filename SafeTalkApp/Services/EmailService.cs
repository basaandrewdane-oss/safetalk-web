using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
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

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtpClient.EnableSsl = true;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(message.From ?? smtpUser);
                    mailMessage.Subject = message.Subject;
                    mailMessage.Body = message.Body;
                    mailMessage.IsBodyHtml = message.IsBodyHtml;
                    mailMessage.To.Add(message.To);

                    try
                    {
                        smtpClient.Send(mailMessage);
                    }
                    catch (System.Exception ex)
                    {
                        throw new ApplicationException("Error sending email", ex);
                    }
                }
            }
        }

        // Account Emails
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
        public void SendDoctorVerifiedAccount(UserTblModel doctor)
        {
            var subject = "Your Doctor Account Has Been Verified";
            var body = $@"
            <p>Dear Dr. {doctor.firstName} {doctor.lastName},</p>
            <p>
                We are pleased to inform you that your SafeTalk doctor account has been successfully verified 
                and is now active.
            </p>
            <p>
                You can now log in and start managing your appointments and patient consultations.
            </p>
            <p>
                Thank you for being part of the SafeTalk community.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }


        // Appointment Emails
        // New Appointment Notification Email (to Doctor)
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
        // Appointment Confirmation Email (to Patient)
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
        // Appointment Approved Email (to Patient)
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
        // Appointment Approved Email (to Doctor)
        public void SendDoctorAppointmentApproved(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = "Appointment Confirmation",
                Body = $"Hi {doctor.firstName},<br><br>" +
                       $"Your appointment with Dr. {patient.firstName} {patient.lastName} " +
                       $"is confirmed on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm}.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }
        // Appointment Cancelled Email (to Doctor)
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
        // Appointment Cancelled Email (to Patient)
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
        // Appointment Rejected Email (to Patient)
        public void SendPatientAppointmentRejected(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = "Appointment Rejected",
                Body = $"Hi {patient.firstName},<br><br>" +
                       $"We regret to inform you that your appointment request with Dr. {doctor.firstName} {doctor.lastName} " +
                       $"on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm} has been rejected.<br><br>" +
                       $"Please consider scheduling with another available doctor.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }
        // Appointment Rejected Email (to Doctor)
        public void SendDoctorAppointmentRejected(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment)
        {
            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = "Appointment Rejected",
                Body = $"Dear Dr. {doctor.firstName} {doctor.lastName},<br><br>" +
                       $"You have rejected the appointment request from {patient.firstName} {patient.lastName} " +
                       $"on {appointment.date:yyyy-MM-dd} at {appointment.startTime:hh\\:mm}.<br><br>" +
                       $"Best regards,<br>SafeTalk Team"
            });
        }

        // Referrals
        public void SendReferralCreatedEmail(UserTblModel patient, UserTblModel doctor, ReferralTblModel referral)
        {
            var subject = "New Referral Created for You";
            var body = $@"
            <p>Dear {patient.firstName},</p>
            <p>
                Dr. {doctor.firstName} {doctor.lastName} has created a referral for you
                to {referral.sentTo}.
            </p>
            <p>
                <strong>Reason:</strong> {referral.reason}<br/>
                <strong>Urgency:</strong> {(UrgencyLevel)referral.urgencyLevel}<br/>
                <strong>Date Issued:</strong> {referral.dateCreated:MMMM dd, yyyy}
            </p>
            <p>
                You can log in to your SafeTalk account to view more details about this referral.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }

        // Payment

        public void SendPaymentSubmittedEmail(UserTblModel admin, AppointmentsTblModel appointment, PaymentTblModel payment, UserTblModel patient, UserTblModel doctor)
        {
            var subject = $"New Payment Submitted – Appointment #{appointment.appointmentID}";
            var body = $@"
                <p>Dear Admin,</p>
                <p>
                    A new payment has been submitted for verification.
                </p>
                <p>
                    <strong>Appointment ID:</strong> {appointment.appointmentID}<br/>
                    <strong>Patient:</strong> {patient?.firstName} {patient?.lastName}<br/>
                    <strong>Doctor:</strong> Dr. {doctor?.firstName} {doctor?.lastName}<br/>
                    <strong>Amount:</strong> ₱{payment.amount:N2}<br/>
                    <strong>Date Submitted:</strong> {payment.paymentDate:MMMM dd, yyyy hh:mm tt}<br/>
                    <strong>Status:</strong> {payment.status}
                </p>
                <p>
                    You can log in to the admin dashboard to review and verify this payment.<br/>
                    <a href='https://safetalk.com/admin/payments' style='color:#007bff;'>View Payment Details</a>
                </p>
                <p>– SafeTalk System</p>";

            SendEmail(new EmailMessageDTO
            {
                To = admin.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }
        public void SendPaymentVerifiedEmailToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, PaymentTblModel payment)
        {
            var subject = "Your Payment Has Been Verified";
            var body = $@"
            <p>Dear {patient.firstName},</p>
            <p>
                Your payment for your consultation with Dr. {doctor.firstName} {doctor.lastName}
                has been verified successfully.
            </p>
            <p>
                <strong>Appointment Date:</strong> {appointment.date:MMMM dd, yyyy}<br/>
                <strong>Time:</strong> {appointment.startTime:hh\\:mm tt} – {appointment.endTime:hh\\:mm tt}<br/>
                <strong>Amount Paid:</strong> ₱{payment.amount:N2}<br/>
                <strong>Status:</strong> Verified
            </p>
            <p>
                You can now attend your appointment as scheduled.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }
        public void SendPaymentVerifiedEmailToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, PaymentTblModel payment)
        {
            var subject = "A Patient’s Payment Has Been Verified";
            var body = $@"
            <p>Dear Dr. {doctor.firstName},</p>
            <p>
                The payment for your upcoming appointment with
                {patient.firstName} {patient.lastName} has been verified.
            </p>
            <p>
                <strong>Appointment Date:</strong> {appointment.date:MMMM dd, yyyy}<br/>
                <strong>Time:</strong> {appointment.startTime:hh\\:mm tt} – {appointment.endTime:hh\\:mm tt}<br/>
                <strong>Amount:</strong> ₱{payment.amount:N2}<br/>
                <strong>Status:</strong> Paid and Confirmed
            </p>
            <p>
                You may now prepare for the consultation.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }
        public void SendPayPalPaymentConfirmationToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, PaymentTblModel payment)
        {
            var subject = "Your Payment Was Successful!";
            var body = $@"
            <p>Dear {patient.firstName},</p>
            <p>
                Your payment for your consultation with Dr. {doctor.firstName} {doctor.lastName}
                has been successfully processed via PayPal.
            </p>
            <p>
                <strong>Appointment Date:</strong> {appointment.date:MMMM dd, yyyy}<br/>
                <strong>Time:</strong> {DateTime.Today.Add(appointment.startTime):hh:mm tt} – {DateTime.Today.Add(appointment.endTime):hh:mm tt}<br/>
                <strong>Amount Paid:</strong> {payment.amount:C}<br/>
                <strong>Transaction ID:</strong> {payment.transactionId}<br/>
                <strong>Status:</strong> Confirmed
            </p>
            <p>
                You can now attend your appointment as scheduled.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }
        public void SendPayPalPaymentNotificationToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, PaymentTblModel payment)
        {
            var subject = "Patient Payment Confirmed";
            var body = $@"
            <p>Dear Dr. {doctor.firstName},</p>
            <p>
                Your patient {patient.firstName} {patient.lastName} has successfully paid for the upcoming consultation.
            </p>
            <p>
                <strong>Appointment Date:</strong> {appointment.date:MMMM dd, yyyy}<br/>
                <strong>Time:</strong> {DateTime.Today.Add(appointment.startTime):hh:mm tt} – {DateTime.Today.Add(appointment.endTime):hh:mm tt}<br/>
                <strong>Amount:</strong> {payment.amount:C}<br/>
                <strong>Transaction ID:</strong> {payment.transactionId}<br/>
                <strong>Status:</strong> Paid
            </p>
            <p>
                You may now proceed with the scheduled appointment.
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }

        // Transcription
        public void SendTranscriptionReadyToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, string transcriptFileName)
        {
            var subject = "Your Consultation Transcript is Ready!";
            var body = $@"
            <p>Dear {patient.firstName},</p>
            <p>
                Your consultation with Dr. {doctor.firstName} {doctor.lastName} on 
                <strong>{appointment.date:MMMM dd, yyyy}</strong> has been successfully transcribed.
            </p>
            <p>
                You can now view or download the transcript by visiting your SafeTalk dashboard.
            </p>
            <p>
                <strong>Appointment Time:</strong> {DateTime.Today.Add(appointment.startTime):hh:mm tt} – {DateTime.Today.Add(appointment.endTime):hh:mm tt}<br/>
                <strong>Transcript File:</strong> {transcriptFileName}
            </p>
            <p>
                <a href='/Uploads/Transcripts/{transcriptFileName}' target='_blank'>Click here to view transcript</a>
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = patient.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }
        public void SendTranscriptionReadyToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, string transcriptFileName)
        {
            var subject = "Consultation Transcript Generated";
            var body = $@"
            <p>Dear Dr. {doctor.firstName} {doctor.lastName},</p>
            <p>
                The consultation with your patient <strong>{patient.firstName} {patient.lastName}</strong> 
                on <strong>{appointment.date:MMMM dd, yyyy}</strong> has been successfully transcribed.
            </p>
            <p>
                You can review the transcript by logging into your SafeTalk dashboard.
            </p>
            <p>
                <strong>Appointment Time:</strong> {DateTime.Today.Add(appointment.startTime):hh:mm tt} – {DateTime.Today.Add(appointment.endTime):hh:mm tt}<br/>
                <strong>Transcript File:</strong> {transcriptFileName}
            </p>
            <p>
                <a href='/Uploads/Transcripts/{transcriptFileName}' target='_blank'>Click here to view transcript</a>
            </p>
            <p>– SafeTalk Team</p>";

            SendEmail(new EmailMessageDTO
            {
                To = doctor.email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            });
        }

    }
}