using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class FAQsTblMap : EntityTypeConfiguration<FAQsTblModel>
    {
        public FAQsTblMap()
        {
            ToTable("faqs_tbl");
            HasKey(t => t.faqID);
        }
    }
}