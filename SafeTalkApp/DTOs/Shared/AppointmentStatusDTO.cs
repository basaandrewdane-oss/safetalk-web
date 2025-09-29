using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Shared
{
    public class AppointmentStatusDTO
    {
        public int status { get; set; }
        public DateTime endTime { get; set; }
    }
}