using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Users.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
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

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Nickname).IsUnique();
        builder.HasIndex(u => u.IsBanned);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasMany(ur => ur.Users)
            .WithOne(u => u.UserRole)
            .HasForeignKey(u => u.UserRoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.HasOne(us => us.User)
            .WithMany(u => u.Skills)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(us => us.UserId);
        builder.HasIndex(us => us.SkillName);
    }
}

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.HasOne(ui => ui.User)
            .WithMany(u => u.Interests)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ui => ui.UserId);
        builder.HasIndex(ui => ui.InterestName);
    }
}
