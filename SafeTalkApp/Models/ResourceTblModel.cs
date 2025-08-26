using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ResourceTblModel
    {
        public int resourceID { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public DateTime publishedDate { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}