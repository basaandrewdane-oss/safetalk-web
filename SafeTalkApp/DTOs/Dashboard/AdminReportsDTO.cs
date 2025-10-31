using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Dashboard
{
    public class AdminReportsDTO
    {
        public Dictionary<string, int> StatusCounts { get; set; }
        public List<TrendDTO> AppointmentTrends { get; set; }
        public List<UserGrowthDTO> UserGrowth { get; set; }
        public List<ResourceTrendDTO> ResourceUploads { get; set; }
    }
}