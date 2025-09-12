using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class PromptsTblMap : EntityTypeConfiguration<PromptsTblModel>
    {
        public PromptsTblMap()
        {
            // Table & Column Mappings
            ToTable("prompts_tbl");
            // Primary Key
            HasKey(t => t.promptID);
        }
    }
}