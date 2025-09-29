using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class ResendVerficationResultDTO
    {
        public string Email { get; set; }
        public DateTime? Expiry { get; set; }
    }
}