using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class TermsTblMap : EntityTypeConfiguration<TermsTblModel>
    {
        public TermsTblMap()
        {
            ToTable("terms_tbl");
            HasKey(t => t.termID);
        }
    }
}