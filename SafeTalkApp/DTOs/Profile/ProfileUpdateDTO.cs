using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Profile
{
    public class ProfileUpdateDTO
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string specialization { get; set; }
        public string contactNumber { get; set; }

        // For profile picture
        public HttpPostedFileBase file { get; set; }
    }
}