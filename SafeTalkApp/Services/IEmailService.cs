using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;

namespace SafeTalkApp.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailMessageDTO message);

        // Account Emails
        void SendVerificationEmail(string toEmail, string verificationLink);
        void SendDoctorVerifiedAccount(UserTblModel doctor);

        // Appointment Emails
        void SendDoctorAppointmentNotification(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);
        void SendPatientAppointmentConfirmation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendPatientAppointmentApproved(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendDoctorAppointmentApproved(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);
        void SendDoctorAppointmentCancellation(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);
        void SendPatientAppointmentCancellation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendPatientAppointmentRejected(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendDoctorAppointmentRejected(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);

        // Referrals
        void SendReferralCreatedEmail(UserTblModel patient, UserTblModel doctor, ReferralTblModel referral);

        // Payments
        void SendPaymentSubmittedEmail(UserTblModel admin, AppointmentsTblModel appointment, PaymentTblModel payment, UserTblModel patient, UserTblModel doctor);
        void SendPaymentVerifiedEmailToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, PaymentTblModel payment);
        void SendPaymentVerifiedEmailToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, PaymentTblModel payment);
        void SendPayPalPaymentConfirmationToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, PaymentTblModel payment);
        void SendPayPalPaymentNotificationToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, PaymentTblModel payment);

        // Transcriptions
        void SendTranscriptionReadyToPatient(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment, string transcriptFileName);
        void SendTranscriptionReadyToDoctor(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment, string transcriptFileName);
    }
}