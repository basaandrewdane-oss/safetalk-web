using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class FeedbackTblMap : EntityTypeConfiguration<FeedbackTblModel>
    {
        public FeedbackTblMap()
        {
            HasKey(t => t.feedbackID);
            ToTable("feedback_tbl");
        }
    }
}