using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ChatMessageTblMap : EntityTypeConfiguration<ChatMessageTblModel>
    {
        public ChatMessageTblMap()
        {
            ToTable("chat_message_tbl");
            HasKey(x => x.messageID);
        }
    }
}