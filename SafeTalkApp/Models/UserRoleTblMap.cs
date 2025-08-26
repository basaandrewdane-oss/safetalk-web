using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserRoleTblMap : EntityTypeConfiguration<UserRoleTblModel>
    {
        public UserRoleTblMap()
        {
            ToTable("user_role_tbl");
            HasKey(x => x.ID);
        }
    }
}