using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class DaysOfWeekTblMap : EntityTypeConfiguration<DaysOfWeekTblModel>
    {
        public DaysOfWeekTblMap()
        {
            ToTable("days_of_week_tbl");
            HasKey(d => d.dayID);
        }
    }
}