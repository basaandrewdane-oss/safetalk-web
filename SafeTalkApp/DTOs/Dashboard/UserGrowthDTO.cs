using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Dashboard
{
    public class UserGrowthDTO
    {
        public string Month { get; set; }
        public int PatientCount { get; set; }
        public int DoctorCount { get; set; }
    }
}