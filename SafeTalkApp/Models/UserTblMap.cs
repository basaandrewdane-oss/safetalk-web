using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserTblMap : EntityTypeConfiguration<UserTblModel>
    {
        public UserTblMap()
        {
            ToTable("user_tbl");
            HasKey(u => u.userID);
        }
    }
}