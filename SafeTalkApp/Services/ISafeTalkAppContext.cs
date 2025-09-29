using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public interface ISafeTalkAppContext : IDisposable
    {
        DbSet<AppointmentsTblModel> appointments_tbl { get; set; }
        DbSet<ChatMessageTblModel> chat_message_tbl { get; set; }
        DbSet<DaysOfWeekTblModel> days_of_week_tbl { get; set; }
        DbSet<FAQsTblModel> faqs_tbl { get; set; }
        DbSet<GenderTblModel> gender_tbl { get; set; }
        DbSet<PaymentTblModel> payment_tbl { get; set; }
        DbSet<PromptsTblModel> prompts_tbl { get; set; }
        DbSet<ResourceTblModel> resource_tbl { get; set; }
        DbSet<RoleTblModel> role_tbl { get; set; }
        DbSet<UserAvailabilityTblModel> user_availability_tbl { get; set; }
        DbSet<UserRoleTblModel> user_role_tbl { get; set; }
        DbSet<UserTblModel> user_tbl { get; set; }

        int SaveChanges();
    }
}