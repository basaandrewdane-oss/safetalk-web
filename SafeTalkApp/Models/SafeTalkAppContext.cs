using MySql.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class SafeTalkAppContext : DbContext, ISafeTalkAppContext
    {
        static SafeTalkAppContext()
        {
            Database.SetInitializer<SafeTalkAppContext>(null);
        }

        public SafeTalkAppContext() : base("name=safetalkappdb") { }

        public virtual DbSet<AppointmentsTblModel> appointments_tbl { get; set; }
        public virtual DbSet<ChatMessageTblModel> chat_message_tbl { get; set; }
        public virtual DbSet<DaysOfWeekTblModel> days_of_week_tbl { get; set; }
        public virtual DbSet<FAQsTblModel> faqs_tbl { get; set; }
        public virtual DbSet<FeedbackTblModel> feedback_tbl { get; set; }
        public virtual DbSet<GenderTblModel> gender_tbl { get; set; }
        public virtual DbSet<PaymentTblModel> payment_tbl { get; set; }
        public virtual DbSet<PromptsTblModel> prompts_tbl { get; set; }
        public virtual DbSet<ReferralTblModel> referrals_tbl { get; set; }
        public virtual DbSet<ResourceTblModel> resource_tbl { get; set; }
        public virtual DbSet<RoleTblModel> role_tbl { get; set; }
        public virtual DbSet<TermsTblModel> terms_tbl { get; set; }
        public virtual DbSet<UserAvailabilityTblModel> user_availability_tbl { get; set; }
        public virtual DbSet<UserRoleTblModel> user_role_tbl { get; set; }
        public virtual DbSet<UserTblModel> user_tbl { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new AppointmentsTblMap());
            modelBuilder.Configurations.Add(new ChatMessageTblMap());
            modelBuilder.Configurations.Add(new DaysOfWeekTblMap());
            modelBuilder.Configurations.Add(new FAQsTblMap());
            modelBuilder.Configurations.Add(new FeedbackTblMap());
            modelBuilder.Configurations.Add(new GenderTblMap());
            modelBuilder.Configurations.Add(new PaymentTblMap());
            modelBuilder.Configurations.Add(new PromptsTblMap());
            modelBuilder.Configurations.Add(new ReferralTblMap());
            modelBuilder.Configurations.Add(new ResourceTblMap());
            modelBuilder.Configurations.Add(new RoleTblMap());
            modelBuilder.Configurations.Add(new TermsTblMap());
            modelBuilder.Configurations.Add(new UserAvailabilityTblMap());
            modelBuilder.Configurations.Add(new UserRoleTblMap());
            modelBuilder.Configurations.Add(new UserTblMap());
        }
    }
}