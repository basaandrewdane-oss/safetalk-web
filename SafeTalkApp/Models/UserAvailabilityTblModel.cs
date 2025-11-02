using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserAvailabilityTblModel
    {
        public int availabilityID { get; set; }
        public int userID { get; set; }
        public int dayID { get; set; }
        public TimeSpan availabilityStart { get; set; }
        public TimeSpan availabilityEnd { get; set; }
        public decimal fee { get; set; }
        public int? slotDuration { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}