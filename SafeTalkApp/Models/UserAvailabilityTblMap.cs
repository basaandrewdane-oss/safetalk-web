using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserAvailabilityTblMap : EntityTypeConfiguration<UserAvailabilityTblModel>
    {
        public UserAvailabilityTblMap()
        {
            ToTable("user_availability_tbl");
            HasKey(u => u.availabilityID);
        }
    }
}