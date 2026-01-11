using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Data.Configurations;

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
