using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Chat.Entities;

namespace GROUPFLOW.Features.Chat.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Entities.Chat>
{
    public void Configure(EntityTypeBuilder<Entities.Chat> builder)
    {
        builder.HasOne(c => c.Project)
            .WithOne(p => p.Chat)
            .HasForeignKey<Entities.Chat>(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ProjectId);
    }
}

public class UserChatConfiguration : IEntityTypeConfiguration<UserChat>
{
    public void Configure(EntityTypeBuilder<UserChat> builder)
    {
        builder.HasOne(uc => uc.Chat)
            .WithMany(c => c.UserChats)
            .HasForeignKey(uc => uc.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserChats)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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
        builder.HasOne(e => e.UserChat)
            .WithMany(uc => uc.Entries)
            .HasForeignKey(e => e.UserChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Sent);
        builder.HasIndex(e => e.Public);
        builder.HasIndex(e => e.UserChatId);
    }
}

public class EntryReactionConfiguration : IEntityTypeConfiguration<EntryReaction>
{
    public void Configure(EntityTypeBuilder<EntryReaction> builder)
    {
        builder.HasOne(er => er.User)
            .WithMany(u => u.EntryReactions)
            .HasForeignKey(er => er.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(er => er.Entry)
            .WithMany(e => e.Reactions)
            .HasForeignKey(er => er.EntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(er => er.Emote)
            .WithMany(em => em.Reactions)
            .HasForeignKey(er => er.EmoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(er => new { er.UserId, er.EmoteId, er.EntryId }).IsUnique();
    }
}

public class ReadByConfiguration : IEntityTypeConfiguration<ReadBy>
{
    public void Configure(EntityTypeBuilder<ReadBy> builder)
    {
        builder.HasOne(rb => rb.Entry)
            .WithMany(e => e.ReadBys)
            .HasForeignKey(rb => rb.EntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rb => rb.User)
            .WithMany(u => u.ReadBys)
            .HasForeignKey(rb => rb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rb => new { rb.UserId, rb.EntryId }).IsUnique();
    }
}

public class SharedFileConfiguration : IEntityTypeConfiguration<SharedFile>
{
    public void Configure(EntityTypeBuilder<SharedFile> builder)
    {
        builder.HasOne(sf => sf.Chat)
            .WithMany(c => c.SharedFiles)
            .HasForeignKey(sf => sf.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sf => sf.ChatId);
    }
}

public class EmoteConfiguration : IEntityTypeConfiguration<Emote>
{
    public void Configure(EntityTypeBuilder<Emote> builder)
    {
        builder.HasIndex(e => e.Name).IsUnique();
    }
}
