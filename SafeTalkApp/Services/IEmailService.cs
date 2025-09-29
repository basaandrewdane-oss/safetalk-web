using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;

namespace SafeTalkApp.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailMessageDTO message);
        void SendDoctorAppointmentNotification(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);
        void SendPatientAppointmentConfirmation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendPatientAppointmentApproved(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendDoctorAppointmentCancellation(UserTblModel doctor, UserTblModel patient, AppointmentsTblModel appointment);
        void SendPatientAppointmentCancellation(UserTblModel patient, UserTblModel doctor, AppointmentsTblModel appointment);
        void SendVerificationEmail(string toEmail, string verificationLink);

    }
}