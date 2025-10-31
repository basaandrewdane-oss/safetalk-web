using SafeTalkApp.DTOs.Reports;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ISafeTalkAppContext _db;

        public ReportsService(ISafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<IEnumerable<ConsultationReportDTO>> GetConsultationReport(int userID)
        {
            try
            {
                var consultations = _db.appointments_tbl
                .Where(a => a.doctorID == userID && a.status == 6)
                .Select(a => new
                {
                    Year = a.date.Year,
                    Month = a.date.Month
                })
                .GroupBy(a => new { a.Year, a.Month })
                .Select(g => new ConsultationReportDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    ConsultationCount = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToList();

                return ApiResponse<IEnumerable<ConsultationReportDTO>>.Ok(consultations, "Report generated successfully.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? string.Empty;
                return ApiResponse<IEnumerable<ConsultationReportDTO>>.Fail(
                    "An error occurred while generating the report. " + ex.Message + " " + innerMessage
                );
            }
        }

        public ApiResponse<IEnumerable<PatientHistoryDTO>> GetPatientHistory(int? patientID, int doctorID)
        {
            try
            {
                if (patientID == null)
                {
                    var patients = _db.appointments_tbl
                        .Where(a => a.doctorID == doctorID)
                        .Join(_db.user_tbl, a => a.patientID, p => p.userID, (a, p) => p)
                        .Distinct()
                        .Select(p => new PatientHistoryDTO
                        {
                            PatientID = p.userID,
                            FullName = p.firstName + " " + p.lastName,
                            Email = p.email,
                            Phone = p.phoneNumber
                        })
                        .ToList();
                    return ApiResponse<IEnumerable<PatientHistoryDTO>>.Ok(patients, "Patients retrieved successfully.");
                }

                var history = _db.appointments_tbl
                    .Where(a => a.doctorID == doctorID && a.patientID == patientID)
                    .Join(_db.user_tbl, a => a.patientID, p => p.userID, (a, p) => new { appointment = a, patient = p })
                    .Select(x => new PatientHistoryDTO
                    {
                        PatientID = x.patient.userID,
                        FullName = x.patient.firstName + " " + x.patient.lastName,
                        Email = x.patient.email,
                        Phone = x.patient.phoneNumber,
                        Date = x.appointment.date,
                        Time = x.appointment.startTime + "-" + x.appointment.endTime,
                        Status = x.appointment.status
                    })
                    .OrderByDescending(h => h.Date)
                    .ThenByDescending(h => h.Time)
                    .ToList();
                return ApiResponse<IEnumerable<PatientHistoryDTO>>.Ok(history, "Patient history retrieved successfully.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? string.Empty;
                return ApiResponse<IEnumerable<PatientHistoryDTO>>.Fail(
                    "An error occurred while retrieving patient history. " + ex.Message + " " + innerMessage
                );
            }
        }

        public ApiResponse<IEnumerable<DoctorHistoryDTO>> GetDoctorHistory(int? doctorID, int patientID)
        {
            try
            {
                if (doctorID == null)
                {
                    var doctors = _db.appointments_tbl
                        .Where(a => a.patientID == patientID)
                        .Join(_db.user_tbl, a => a.doctorID, d => d.userID, (a, d) => d)
                        .Distinct()
                        .Select(d => new DoctorHistoryDTO
                        {
                            DoctorID = d.userID,
                            FullName = d.firstName + " " + d.lastName,
                            Email = d.email,
                            Phone = d.phoneNumber
                        })
                        .ToList();
                    return ApiResponse<IEnumerable<DoctorHistoryDTO>>.Ok(doctors, "Doctors retrieved successfully.");
                }
                var history = _db.appointments_tbl
                    .Where(a => a.patientID == patientID && a.doctorID == doctorID)
                    .Join(_db.user_tbl, a => a.doctorID, d => d.userID, (a, d) => new { appointment = a, doctor = d })
                    .Select(x => new DoctorHistoryDTO
                    {
                        DoctorID = x.doctor.userID,
                        FullName = x.doctor.firstName + " " + x.doctor.lastName,
                        Email = x.doctor.email,
                        Phone = x.doctor.phoneNumber,
                        Date = x.appointment.date,
                        Time = x.appointment.startTime + "-" + x.appointment.endTime,
                        Status = x.appointment.status
                    })
                    .OrderByDescending(h => h.Date)
                    .ThenByDescending(h => h.Time)
                    .ToList();
                return ApiResponse<IEnumerable<DoctorHistoryDTO>>.Ok(history, "Doctor history retrieved successfully.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? string.Empty;
                return ApiResponse<IEnumerable<DoctorHistoryDTO>>.Fail(
                    "An error occurred while retrieving doctor history. " + ex.Message + " " + innerMessage
                );
            }
        }

        public ApiResponse<IEnumerable<MissedAppointmentsDTO>> GetMissedAppointments(int userID)
        {
            try
            {
                var appointments = (from a in _db.appointments_tbl
                                    join p in _db.user_tbl on a.patientID equals p.userID
                                    join d in _db.user_tbl on a.doctorID equals d.userID
                                    where a.status == 7 && (a.patientID == userID || a.doctorID == userID)
                                    select new MissedAppointmentsDTO
                                    {
                                        patientName = p.firstName + " " + p.lastName,
                                        doctorName = d.firstName + " " + d.lastName,
                                        date = a.date,
                                        startTime = a.startTime,
                                        endTime = a.endTime,
                                        status = a.status
                                    }).ToList();

                return ApiResponse<IEnumerable<MissedAppointmentsDTO>>.Ok(appointments, "Missed appointments retrieved successfully.");

            }
            catch (Exception)
            {
                return ApiResponse<IEnumerable<MissedAppointmentsDTO>>.Fail("An error occurred while retrieving missed appointments.");
            }
        }
    }
}