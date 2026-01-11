using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Relationships
        builder.HasOne(u => u.BannedBy)
            .WithMany()
            .HasForeignKey(u => u.BannedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ProfilePicBlob)
            .WithMany()
            .HasForeignKey(u => u.ProfilePicBlobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.BannerPicBlob)
            .WithMany()
            .HasForeignKey(u => u.BannerPicBlobId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Nickname).IsUnique();
        builder.HasIndex(u => u.IsBanned);
    }
}
