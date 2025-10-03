using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Reports
{
    public class ConsultationReportDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int ConsultationCount { get; set; }
        public string Label => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}