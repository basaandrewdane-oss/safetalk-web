using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class LogInViewModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public int roleID { get; set; } 
    }
}