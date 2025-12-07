using SafeTalkApp.DTOs.Reports;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IReportsService
    {
        ApiResponse<IEnumerable<ConsultationReportDTO>> GetConsultationReport(int userID);
        ApiResponse<IEnumerable<PatientHistoryDTO>> GetPatientHistory(int? patientID, int doctorID);
        ApiResponse<IEnumerable<DoctorHistoryDTO>> GetDoctorHistory(int? doctorID, int patientID);
        ApiResponse<IEnumerable<MissedAppointmentsDTO>> GetMissedAppointments(int userID);
    }
}