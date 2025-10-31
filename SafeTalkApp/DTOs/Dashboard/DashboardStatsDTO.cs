using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Dashboard
{
    public class DashboardStatsDTO
    {
        public int UpcomingAppointments { get; set; }
        public int ActiveConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int Resources { get; set; }

        // Chart breakdown
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int PaidCount { get; set; }
        public int CompletedCount { get; set; }
        public int MissedCount { get; set; }

        // Line chart data
        public List<ConsultationTrendDTO> ConsultationTrends { get; set; }
    }
}