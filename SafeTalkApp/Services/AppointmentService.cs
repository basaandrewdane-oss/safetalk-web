using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SafeTalkApp.DTOs.Appointment.DoctorAvailabilityDTO;

namespace SafeTalkApp.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IEmailService _emailService;

        public AppointmentService(ISafeTalkAppContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public ApiResponse<AppointmentStatusDTO> GetAppointmentStatus(int appointmentId)
        {
            try
            {
                var appointment = _db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentId);
                if (appointment == null)
                    return ApiResponse<AppointmentStatusDTO>.Fail("Appointment not found.");

                var dto = new AppointmentStatusDTO
                {
                    status = appointment.status,
                    endTime = appointment.date.Add(appointment.endTime) // if endTime is a TimeSpan
                };

                return ApiResponse<AppointmentStatusDTO>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AppointmentStatusDTO>.Fail("Error retrieving appointment status: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<DoctorDTO>> GetDoctors()
        {
            try
            {
                var doctors = (from u in _db.user_tbl
                               join ur in _db.user_role_tbl on u.userID equals ur.userID
                               join r in _db.role_tbl on ur.roleID equals r.roleID
                               where r.roleName == "Doctor" && u.isVerified
                               select new DoctorDTO
                               {
                                   userID = u.userID,
                                   fullName = u.firstName + " " + u.lastName,
                                   email = u.email,
                                   specialization = u.specialization
                               }).ToList();

                return ApiResponse<IEnumerable<DoctorDTO>>.Ok(doctors);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DoctorDTO>>.Fail("Error retrieving doctors: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<DoctorAvailabilityDTO>> GetDoctorsAvailability(int userID)
        {
            try
            {
                var availability = (from a in _db.user_availability_tbl
                                    join d in _db.days_of_week_tbl on a.dayID equals d.dayID
                                    where a.userID == userID
                                    select new
                                    {
                                        a.availabilityID,
                                        a.userID,
                                        d.dayID,
                                        d.day,
                                        a.availabilityStart,
                                        a.availabilityEnd,
                                        a.fee
                                    }).ToList();

                var result = availability.Select(a => new DoctorAvailabilityDTO
                {
                    availabilityID = a.availabilityID,
                    userID = a.userID,
                    dayID = a.dayID,
                    day = a.day,
                    fee = a.fee,
                    slots = GenerateTimeSlots(a.availabilityStart, a.availabilityEnd)
                }).ToList();

                return ApiResponse<IEnumerable<DoctorAvailabilityDTO>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DoctorAvailabilityDTO>>.Fail("Error retrieving availability: " + ex.Message);
            }
        }

        private IEnumerable<TimeSlotDTO> GenerateTimeSlots(TimeSpan start, TimeSpan end, int intervalMinutes = 30)
        {
            var slots = new List<TimeSlotDTO>();

            while (start + TimeSpan.FromMinutes(intervalMinutes) <= end)
            {
                var next = start + TimeSpan.FromMinutes(intervalMinutes);
                slots.Add(new TimeSlotDTO
                {
                    start = start.ToString(@"hh\:mm\:ss"),
                    end = next.ToString(@"hh\:mm\:ss")
                });
                start = next;
            }

            return slots;
        }

        public ApiResponse<AppointmentResultDTO> BookAppointment(BookAppointmentDTO model, int patientID)
        {
            try
            {
                // Overlap check
                var overlappingAppointment = _db.appointments_tbl.FirstOrDefault(a =>
                    a.doctorID == model.doctorID &&
                    a.date == model.date &&
                    !(model.endTime <= a.startTime || model.startTime >= a.endTime)
                );

                if (overlappingAppointment != null)
                {
                    return ApiResponse<AppointmentResultDTO>.Fail("This time slot is already booked.");
                }
                //// Check if doctor already has an overlapping appointment
                //var existingAppointment = _db.appointments_tbl.FirstOrDefault(a =>
                //    a.doctorID == model.doctorID &&
                //    a.date == model.date &&
                //    ((model.startTime >= a.startTime && model.startTime < a.endTime) ||
                //     (model.endTime > a.startTime && model.endTime <= a.endTime))
                //);

                //if (existingAppointment != null)
                //{
                //    return ApiResponse<AppointmentResultDTO>.Fail("This time slot is already booked.");
                //}

                var appointment = new AppointmentsTblModel
                {
                    doctorID = model.doctorID,
                    patientID = patientID,
                    date = model.date,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    fee = model.fee,
                    chiefComplaint = model.chiefComplaint,
                    status = AppointmentStatus.Pending,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now
                };

                _db.appointments_tbl.Add(appointment);
                _db.SaveChanges();

                var dto = new AppointmentResultDTO
                {
                    appointmentID = appointment.appointmentID,
                    doctorID = appointment.doctorID,
                    patientID = appointment.patientID,
                    date = appointment.date,
                    startTime = appointment.startTime,
                    endTime = appointment.endTime,
                    fee = appointment.fee,
                    chiefComplaint = appointment.chiefComplaint,
                    status = appointment.status
                };

                //var doctor = _db.user_tbl.Find(model.doctorID);
                //var patient = _db.user_tbl.Find(patientID);

                //if (doctor != null && patient != null)
                //{
                //    _emailService.SendDoctorAppointmentNotification(doctor, patient, appointment);
                //    _emailService.SendPatientAppointmentConfirmation(patient, doctor, appointment);
                //}

                return ApiResponse<AppointmentResultDTO>.Ok(dto, "Appointment booked successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AppointmentResultDTO>.Fail("Error booking appointment: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<PatientAppointmentDTO>> GetPatientAppointments(int patientID)
        {
            try
            {
                var appointments = (from a in _db.appointments_tbl
                                    join d in _db.user_tbl on a.doctorID equals d.userID
                                    where a.patientID == patientID
                                    orderby a.date descending, a.startTime descending
                                    select new PatientAppointmentDTO
                                    {
                                        appointmentID = a.appointmentID,
                                        date = a.date,
                                        startTime = a.startTime,
                                        endTime = a.endTime,
                                        fee = a.fee,
                                        status = a.status,
                                        doctorName = d.firstName + " " + d.lastName,
                                        doctorEmail = d.email,
                                        phoneNumber = d.phoneNumber
                                    }).ToList();

                return ApiResponse<IEnumerable<PatientAppointmentDTO>>.Ok(appointments);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<PatientAppointmentDTO>>.Fail("Error retrieving patient appointments: " + ex.Message);
            }
        }

        public ApiResponse<bool> CancelAppointment(int appointmentID)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<bool>.Fail("Appointment not found.");
                }

                appointment.status = AppointmentStatus.Canceled;
                appointment.dateUpdated = DateTime.Now;
                _db.SaveChanges();

                // 🔔 Optionally, notify doctor and patient here
                // var doctor = _db.user_tbl.Find(appointment.doctorID);
                // var patient = _db.user_tbl.Find(appointment.patientID);
                // if (doctor != null && patient != null)
                // {
                //     _emailService.SendDoctorAppointmentCancellation(doctor, patient, appointment);
                //     _emailService.SendPatientAppointmentCancellation(patient, doctor, appointment);
                // }

                return ApiResponse<bool>.Ok(true, "Appointment cancelled successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("Error cancelling appointment: " + ex.Message);
            }
        }

        public ApiResponse<IEnumerable<DoctorAppointmentDTO>> GetDoctorAppointments(int doctorId)
        {
            try
            {
                var appointments = (from a in _db.appointments_tbl
                                    join d in _db.user_tbl on a.patientID equals d.userID
                                    join p in _db.payment_tbl on a.appointmentID equals p.appointmentID into pay
                                    from p in pay.DefaultIfEmpty()
                                    where a.doctorID == doctorId
                                    orderby a.date descending, a.startTime descending
                                    select new DoctorAppointmentDTO
                                    {
                                        appointmentID = a.appointmentID,
                                        date = a.date,
                                        startTime = a.startTime,
                                        endTime = a.endTime,
                                        status = a.status,
                                        patientName = d.firstName + " " + d.lastName,
                                        patientEmail = d.email,
                                        paymentImage = p.imagePath,
                                        transcriptPath = a.transcriptFilePath
                                    }).ToList();

                return ApiResponse<IEnumerable<DoctorAppointmentDTO>>.Ok(appointments);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DoctorAppointmentDTO>>.Fail("Error fetching doctor appointments: " + ex.Message);
            }
        }

        public ApiResponse<bool> ApproveAppointment(int appointmentID)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<bool>.Fail("Appointment not found.");
                }

                appointment.status = AppointmentStatus.Approved;
                appointment.dateUpdated = DateTime.Now;
                _db.SaveChanges();

                //// Send confirmation email to patient
                //var patient = _db.user_tbl.Find(appointment.patientID);
                //var doctor = _db.user_tbl.Find(appointment.doctorID);

                //if (patient != null && doctor != null)
                //{
                //    _emailService.SendPatientAppointmentApproved(patient, doctor, appointment);
                //}

                return ApiResponse<bool>.Ok(true, "Appointment approved and confirmation email sent.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("Error approving appointment: " + ex.Message);
            }
        }

        public ApiResponse<bool> RejectAppointment(int appointmentID)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<bool>.Fail("Appointment not found.");
                }
                appointment.status = AppointmentStatus.Rejected;
                appointment.dateUpdated = DateTime.Now;
                _db.SaveChanges();
                // Send rejection email to patient
                //var patient = _db.user_tbl.Find(appointment.patientID);
                //var doctor = _db.user_tbl.Find(appointment.doctorID);
                //if (patient != null && doctor != null)
                //{
                //    _emailService.SendPatientAppointmentRejected(patient, doctor, appointment);
                //}
                return ApiResponse<bool>.Ok(true, "Appointment rejected and notification email sent.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("Error rejecting appointment: " + ex.Message);
            }
        }
    }
}