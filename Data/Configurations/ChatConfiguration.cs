using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Data.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.HasIndex(c => c.ProjectId);
    }
}

public class UserChatConfiguration : IEntityTypeConfiguration<UserChat>
{
    public void Configure(EntityTypeBuilder<UserChat> builder)
    {
        builder.HasIndex(uc => uc.UserId);
        builder.HasIndex(uc => uc.ChatId);
        builder.HasIndex(uc => new { uc.ChatId, uc.UserId })
            .HasDatabaseName("IX_UserChats_ChatId_UserId");
    }
}

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasIndex(e => e.Sent);
        builder.HasIndex(e => e.Public);
        builder.HasIndex(e => e.UserChatId);
    }
}
