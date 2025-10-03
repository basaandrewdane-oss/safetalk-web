using SafeTalkApp.DTOs.Dashboard;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IDashboardService
    {
        ApiResponse<DashboardStatsDTO> GetDashboardStats(int userId, string role);
    }
}