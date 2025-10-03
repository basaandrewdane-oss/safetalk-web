using SafeTalkApp.DTOs.Dashboard;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
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
            var dto = new DashboardStatsDTO();

            if (role == "User" || role == "Doctor")
            {
                dto.appointments = _db.appointments_tbl
                    .Count(a => a.patientID == userId || a.doctorID == userId);

                dto.consultations = _db.appointments_tbl
                    .Where(a => a.status == 3)
                    .Count(c => c.patientID == userId || c.doctorID == userId);
            }

            dto.resources = _db.resource_tbl.Count();

            return ApiResponse<DashboardStatsDTO>.Ok(dto);
        }
    }
}