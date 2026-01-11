using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Projects.Entities;

namespace GROUPFLOW.Features.Projects.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ImageBlob)
            .WithMany()
            .HasForeignKey(p => p.ImageBlobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.BannerBlob)
            .WithMany()
            .HasForeignKey(p => p.BannerBlobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.IsPublic);
        builder.HasIndex(p => p.Created);
        builder.HasIndex(p => p.LastUpdated);
        builder.HasIndex(p => new { p.IsPublic, p.LastUpdated })
            .HasDatabaseName("IX_Projects_IsPublic_LastUpdated");
    }
}

public class UserProjectConfiguration : IEntityTypeConfiguration<UserProject>
{
    public void Configure(EntityTypeBuilder<UserProject> builder)
    {
        builder.HasKey(up => new { up.UserId, up.ProjectId });

        builder.HasOne(up => up.User)
            .WithMany(u => u.ProjectCollaborations)
            .HasForeignKey(up => up.UserId);

        builder.HasOne(up => up.Project)
            .WithMany(p => p.Collaborators)
            .HasForeignKey(up => up.ProjectId);

        builder.HasIndex(up => up.UserId);
        builder.HasIndex(up => up.ProjectId);
    }
}

public class ProjectEventConfiguration : IEntityTypeConfiguration<ProjectEvent>
{
    public void Configure(EntityTypeBuilder<ProjectEvent> builder)
    {
        builder.HasOne(pe => pe.Project)
            .WithMany(p => p.Events)
            .HasForeignKey(pe => pe.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.CreatedBy)
            .WithMany(u => u.CreatedEvents)
            .HasForeignKey(pe => pe.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pe => pe.ProjectId);
        builder.HasIndex(pe => pe.EventDate);
    }
}

public class ProjectInvitationConfiguration : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.HasOne(pi => pi.Project)
            .WithMany()
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.Inviting)
            .WithMany()
            .HasForeignKey(pi => pi.InvitingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.Invited)
            .WithMany()
            .HasForeignKey(pi => pi.InvitedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => pi.ProjectId);
        builder.HasIndex(pi => pi.InvitedId);
    }
}

public class ProjectRecommendationConfiguration : IEntityTypeConfiguration<ProjectRecommendation>
{
    public void Configure(EntityTypeBuilder<ProjectRecommendation> builder)
    {
        builder.HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.Project)
            .WithMany()
            .HasForeignKey(pr => pr.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pr => new { pr.UserId, pr.ProjectId }).IsUnique();
    }
}

public class ProjectLikeConfiguration : IEntityTypeConfiguration<ProjectLike>
{
    public void Configure(EntityTypeBuilder<ProjectLike> builder)
    {
        builder.HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pl => pl.Project)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pl => new { pl.UserId, pl.ProjectId }).IsUnique();
    }
}

public class ProjectViewConfiguration : IEntityTypeConfiguration<ProjectView>
{
    public void Configure(EntityTypeBuilder<ProjectView> builder)
    {
        builder.HasOne(pv => pv.User)
            .WithMany()
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pv => pv.Project)
            .WithMany(p => p.Views)
            .HasForeignKey(pv => pv.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => new { pv.UserId, pv.ProjectId, pv.ViewDate }).IsUnique();
    }
}

public class ProjectSkillConfiguration : IEntityTypeConfiguration<ProjectSkill>
{
    public void Configure(EntityTypeBuilder<ProjectSkill> builder)
    {
        builder.HasOne(ps => ps.Project)
            .WithMany(p => p.Skills)
            .HasForeignKey(ps => ps.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ps => ps.ProjectId);
        builder.HasIndex(ps => ps.SkillName);
    }
}

public class ProjectInterestConfiguration : IEntityTypeConfiguration<ProjectInterest>
{
    public void Configure(EntityTypeBuilder<ProjectInterest> builder)
    {
        builder.HasOne(pi => pi.Project)
            .WithMany(p => p.Interests)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pi => pi.ProjectId);
        builder.HasIndex(pi => pi.InterestName);
    }
}
