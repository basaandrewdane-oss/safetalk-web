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

        public ConsultationService(ISafeTalkAppContext db)
        {
            _db = db;
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

        public ApiResponse<IEnumerable<ChatMessageDTO>> GetChatMessages(int appointmentID)
        {
            try
            {
                var messages = _db.chat_message_tbl
                        .Where(m => m.appointmentID == appointmentID)
                        .OrderBy(m => m.sentAt)
                        .Select(m => new ChatMessageDTO
                        {
                            messageID = m.messageID,
                            appointmentID = m.appointmentID,
                            senderID = m.senderID,
                            message = m.message,
                            sentAt = m.sentAt
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
                                     }).ToList();
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Ok(consultations);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AppointmentResultDTO>>.Fail("An error occurred while retrieving consultations: " + ex.Message);
            }
        }
    }
}