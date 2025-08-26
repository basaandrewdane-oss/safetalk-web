using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserRoleTblModel
    {
        public int ID { get; set; }
        public int userID { get; set; }
        public int roleID { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}