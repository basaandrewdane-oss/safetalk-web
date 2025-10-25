using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ReferralTblMap : EntityTypeConfiguration<ReferralTblModel>
    {
        public ReferralTblMap()
        {
            HasKey(r => r.referralID);
            ToTable("referrals_tbl");
        }
    }
}