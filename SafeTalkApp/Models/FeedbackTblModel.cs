using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class FeedbackTblModel
    {
        public int feedbackID { get; set; }
        public string email { get; set; }
        public string feedback { get; set; }
        public DateTime dateCreated { get; set; }
    }
}