using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Friendships.Entities;

namespace GROUPFLOW.Features.Friendships.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasOne(f => f.User)
            .WithMany(u => u.InitiatedFriendships)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Friend)
            .WithMany(u => u.ReceivedFriendships)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.UserId, f.FriendId })
            .IsUnique()
            .HasDatabaseName("IX_Friendships_UserId_FriendId");
        builder.HasIndex(f => f.FriendId);
        builder.HasIndex(f => f.IsAccepted);
    }
}

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasOne(fr => fr.Requester)
            .WithMany()
            .HasForeignKey(fr => fr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fr => fr.Requestee)
            .WithMany()
            .HasForeignKey(fr => fr.RequesteeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fr => fr.RequesterId);
        builder.HasIndex(fr => fr.RequesteeId);
        builder.HasIndex(fr => new { fr.RequesterId, fr.RequesteeId })
            .HasDatabaseName("IX_FriendRequests_RequesterId_RequesteeId");
    }
}

public class FriendRecommendationConfiguration : IEntityTypeConfiguration<FriendRecommendation>
{
    public void Configure(EntityTypeBuilder<FriendRecommendation> builder)
    {
        builder.HasOne(fr => fr.RecommendedFor)
            .WithMany()
            .HasForeignKey(fr => fr.RecommendedForId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fr => fr.RecommendedWho)
            .WithMany()
            .HasForeignKey(fr => fr.RecommendedWhoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fr => new { fr.RecommendedForId, fr.RecommendedWhoId }).IsUnique();
    }
}

public class BlockedUserConfiguration : IEntityTypeConfiguration<BlockedUser>
{
    public void Configure(EntityTypeBuilder<BlockedUser> builder)
    {
        builder.HasOne(bu => bu.User)
            .WithMany()
            .HasForeignKey(bu => bu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bu => bu.Blocked)
            .WithMany()
            .HasForeignKey(bu => bu.BlockedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bu => new { bu.UserId, bu.BlockedUserId }).IsUnique();
        builder.HasIndex(bu => bu.UserId);
    }
}
