using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class PaymentTblMap : EntityTypeConfiguration<PaymentTblModel>
    {
        public PaymentTblMap()
        {
            ToTable("payment_tbl");
            HasKey(p => p.paymentID);
        }
    }
}