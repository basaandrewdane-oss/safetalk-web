using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class TimeSlot
    {
        public string Start { get; set; }
        public string End { get; set; }
        public bool IsBooked { get; set; } = false;
    }
}