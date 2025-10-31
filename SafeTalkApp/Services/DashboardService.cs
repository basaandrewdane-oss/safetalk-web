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
                dto.MissedCount = userAppointments.Count(a => a.status == AppointmentStatus.Missed);

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
                        Count = completedAppointments.Count(a => a.dateUpdated.Date == day.Date)
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

        public ApiResponse<AdminReportsDTO> GetAdminReports()
        {
            var dto = new AdminReportsDTO();

            // 1️⃣ Appointment Status Counts
            var statusList = new Dictionary<string, int>
            {
                { "Pending", AppointmentStatus.Pending },
                { "Approved", AppointmentStatus.Approved },
                { "PaymentSubmitted", AppointmentStatus.PaymentSubmitted },
                { "Paid", AppointmentStatus.Paid },
                { "Rejected", AppointmentStatus.Rejected },
                { "Canceled", AppointmentStatus.Canceled },
                { "Completed", AppointmentStatus.Completed },
                { "Missed", AppointmentStatus.Missed }
            };

            dto.StatusCounts = statusList.ToDictionary(
                kvp => kvp.Key,
                kvp => _db.appointments_tbl.Count(a => a.status == kvp.Value)
            );

            // 2️⃣ Appointment Trends (last 7 days)
            var today = DateTime.Today;
            dto.AppointmentTrends = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .OrderBy(d => d)
                .Select(d => new TrendDTO
                {
                    Date = d.ToString("MMM dd"),
                    Count = _db.appointments_tbl.Count(a => a.dateCreated.Year == d.Year &&
                                        a.dateCreated.Month == d.Month &&
                                        a.dateCreated.Day == d.Day)
                })
                .ToList();

            // 3️⃣ User Growth (last 6 months)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();


            var patientRoleId = _db.role_tbl.FirstOrDefault(r => r.roleName == "User")?.roleID;
            var doctorRoleId = _db.role_tbl.FirstOrDefault(r => r.roleName == "Doctor")?.roleID;
            dto.UserGrowth = last6Months.Select(m => new UserGrowthDTO
            {
                Month = m.ToString("MMM yyyy"),
                PatientCount = _db.user_tbl
                .Join(_db.user_role_tbl, u => u.userID, ur => ur.userID, (u, ur) => new { u, ur })
                .Where(x => x.ur.roleID == patientRoleId &&
                            x.u.dateCreated.Month == m.Month &&
                            x.u.dateCreated.Year == m.Year)
                .Count(),

                DoctorCount = _db.user_tbl
                .Join(_db.user_role_tbl, u => u.userID, ur => ur.userID, (u, ur) => new { u, ur })
                .Where(x => x.ur.roleID == doctorRoleId &&
                            x.u.dateCreated.Month == m.Month &&
                            x.u.dateCreated.Year == m.Year)
                .Count(),
            }).ToList();

            // 4️⃣ Resource Uploads per Month
            dto.ResourceUploads = last6Months.Select(m => new ResourceTrendDTO
            {
                Month = m.ToString("MMM yyyy"),
                Count = _db.resource_tbl.Count(r => r.dateCreated.Month == m.Month && r.dateCreated.Year == m.Year)
            }).ToList();

            return ApiResponse<AdminReportsDTO>.Ok(dto);
            
        }
    }
}