using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Admin
{
    public class FAQsDTO
    {
        public int faqID { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public string keywords { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}