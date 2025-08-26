using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class RoleTblMap : EntityTypeConfiguration<RoleTblModel>
    {
        public RoleTblMap()
        {
            ToTable("role_tbl");
            HasKey(r => r.roleID);
        }
    }
}