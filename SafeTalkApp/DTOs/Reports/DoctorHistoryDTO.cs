using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Reports
{
    public class DoctorHistoryDTO
    {
        public int DoctorID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Status { get; set; }
    }
}