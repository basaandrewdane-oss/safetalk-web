using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Dashboard
{
    public class ConsultationTrendDTO
    {
        public string Date { get; set; }  // "Oct 10" etc.
        public int Count { get; set; }
    }
}