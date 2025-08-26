using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ChatMessageTblModel
    {
        public int messageID { get; set; }
        public int appointmentID { get; set; }
        public int senderID { get; set; }
        public string message { get; set; }
        public DateTime sentAt { get; set; }
    }
}