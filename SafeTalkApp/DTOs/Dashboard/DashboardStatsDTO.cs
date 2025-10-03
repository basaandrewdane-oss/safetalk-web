using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Dashboard
{
    public class DashboardStatsDTO
    {
        public int appointments { get; set; }
        public int consultations { get; set; }
        public int resources { get; set; }
    }
}