using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class AppointmentsTblMap : EntityTypeConfiguration<AppointmentsTblModel>
    {
        public AppointmentsTblMap()
        {
            ToTable("appointments_tbl");
            HasKey(a => a.appointmentID);
        }
    }
}