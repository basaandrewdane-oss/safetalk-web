using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ResourceTblMap : EntityTypeConfiguration<ResourceTblModel>
    {
        public ResourceTblMap()
        {
            ToTable("resource_tbl");
            HasKey(r => r.resourceID);
        }
    }
}