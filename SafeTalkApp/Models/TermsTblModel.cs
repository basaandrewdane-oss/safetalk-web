using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class TermsTblModel
    {
        public int termID { get; set; }
        public string content { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}