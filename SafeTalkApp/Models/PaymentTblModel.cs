using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class PaymentTblModel
    {
        public int paymentID { get; set; }
        public int appointmentID { get; set; }
        public string imagePath { get; set; }
        public int status { get; set; }
        public string transactionId { get; set; }
        public decimal amount { get; set; }
        public DateTime paymentDate { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}