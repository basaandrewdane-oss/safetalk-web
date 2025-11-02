using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Shared
{
    public class TextTranscriptDTO
    {
        public int appointmentId { get; set; }
        public string transcript { get; set; }
    }
}