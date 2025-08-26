using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class GenderTblMap : EntityTypeConfiguration<GenderTblModel>
    {
        public GenderTblMap()
        {
            ToTable("gender_tbl");
            HasKey(g => g.genderID);
        }
    }
}