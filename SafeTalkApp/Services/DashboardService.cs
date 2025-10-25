using SafeTalkApp.DTOs.Dashboard;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{

    public class DashboardService : IDashboardService
    {
        private readonly ISafeTalkAppContext _db;

        public DashboardService(ISafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<DashboardStatsDTO> GetDashboardStats(int userId, string role)
        {
            try
            {
                var dto = new DashboardStatsDTO();

                var userAppointments = _db.appointments_tbl
                    .Where(a => a.patientID == userId || a.doctorID == userId);

                dto.UpcomingAppointments = userAppointments
                    .Count(a => a.status == AppointmentStatus.Pending || a.status == AppointmentStatus.Approved || a.status == AppointmentStatus.PaymentSubmitted);

                dto.ActiveConsultations = userAppointments
                    .Count(a => a.status == AppointmentStatus.Paid);

                dto.CompletedConsultations = userAppointments
                    .Count(a => a.status == AppointmentStatus.Completed);

                // These values will feed the chart
                dto.PendingCount = userAppointments.Count(a => a.status == AppointmentStatus.Pending);
                dto.ApprovedCount = userAppointments.Count(a => a.status == AppointmentStatus.Approved);
                dto.PaidCount = userAppointments.Count(a => a.status == AppointmentStatus.Paid);
                dto.CompletedCount = userAppointments.Count(a => a.status == AppointmentStatus.Completed);

                dto.Resources = _db.resource_tbl.Count();

                // Line chart: consultations completed in the last 7 days
                var today = DateTime.Today;
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var completedAppointments = userAppointments
                .Where(a => a.status == AppointmentStatus.Completed && a.dateUpdated != null)
                .ToList(); // Force EF to execute now — after this, you’re in LINQ-to-Objects


                dto.ConsultationTrends = last7Days
                    .Select(day => new ConsultationTrendDTO
                    {
                        Date = day.ToString("MMM dd"),
                        Count = completedAppointments.Count(a => a.dateUpdated == day.Date)
                    })
                    .ToList();

                return ApiResponse<DashboardStatsDTO>.Ok(dto);
            }
            catch (Exception ex)
            {
                //StackTrace
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return ApiResponse<DashboardStatsDTO>.Fail("An error occurred while fetching dashboard stats: " + ex.Message);
            }
           
        }
    }
}