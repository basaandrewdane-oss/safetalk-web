using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.DTOs.Consultation;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IConsultationService
    {
        ApiResponse<AppointmentResultDTO> GetAppointment(int appointmentID);
        ApiResponse<IEnumerable<ChatMessageDTO>> GetChatMessages(int appointmentID);
        ApiResponse<IEnumerable<AppointmentResultDTO>> GetPatientConsultations(int userID);
        ApiResponse<IEnumerable<AppointmentResultDTO>> GetDoctorConsultations(int userID);
        ApiResponse<bool> CreateReferral(ReferralDTO model);
        ApiResponse<ReferralDTO> GetReferralDetails(int referralID);
    }
}