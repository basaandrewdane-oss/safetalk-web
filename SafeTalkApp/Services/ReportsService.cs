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
                .Select(a => new {
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

        //public ApiResponse<IEnumerable<PatientsDTO>> GetPatients(int doctorID)
        //{
        //    try
        //    {
        //        var patients = _db.appointments_tbl
        //            .Where(a => a.doctorID == doctorID && a.status == 6)
        //            .Select(a => a.patient_tbl)
        //            .Distinct()
        //            .Select(p => new PatientsDTO
        //            {
        //                PatientID = p.patientID,
        //                FullName = p.firstName + " " + p.lastName,
        //                Email = p.email,
        //                Phone = p.phone
        //            })
        //            .ToList();
        //        return ApiResponse<IEnumerable<PatientsDTO>>.Ok(patients, "Patients retrieved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        var innerMessage = ex.InnerException?.Message ?? string.Empty;
        //        return ApiResponse<IEnumerable<PatientsDTO>>.Fail(
        //            "An error occurred while retrieving patients. " + ex.Message + " " + innerMessage
        //        );
        //    }
        //}

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
    }
}