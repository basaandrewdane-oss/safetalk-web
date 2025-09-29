using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class VerifyEmailResultDTO
    {
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
        public string Email { get; set; }
    }
}