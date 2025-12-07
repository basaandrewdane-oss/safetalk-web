using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IAppointmentService
    {
        ApiResponse<AppointmentStatusDTO> GetAppointmentStatus(int appointmentId);
        ApiResponse<IEnumerable<DoctorDTO>> GetDoctors();
        ApiResponse<IEnumerable<DoctorAvailabilityDTO>> GetDoctorsAvailability(int userID);
        ApiResponse<AppointmentResultDTO> BookAppointment(BookAppointmentDTO model, int patientID);
        ApiResponse<IEnumerable<PatientAppointmentDTO>> GetPatientAppointments(int patientID);
        ApiResponse<bool> CancelAppointment(int appointmentID);
        ApiResponse<IEnumerable<DoctorAppointmentDTO>> GetDoctorAppointments(int doctorId);
        ApiResponse<bool> ApproveAppointment(int appointmentID);
        ApiResponse<bool> RejectAppointment(AppointmentResultDTO data);
        ApiResponse<bool> CheckSlotAvailability(BookAppointmentDTO model);
    }
}