using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Moderation.Entities;

namespace GROUPFLOW.Features.Moderation.Configurations;

public class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.HasIndex(m => m.ModeratorId);
        builder.HasIndex(m => m.TargetUserId);
        builder.HasIndex(m => m.CreatedAt);
    }
}
