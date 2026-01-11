using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Project)
            .WithMany(proj => proj.Posts)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SharedPost)
            .WithMany()
            .HasForeignKey(p => p.SharedPostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ImageBlob)
            .WithMany()
            .HasForeignKey(p => p.ImageBlobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.Created);
        builder.HasIndex(p => p.Public);
        builder.HasIndex(p => new { p.Public, p.Created }).HasDatabaseName("IX_Posts_Public_Created");
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.SharedPostId);
    }
}

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pl => new { pl.UserId, pl.PostId })
            .IsUnique()
            .HasDatabaseName("IX_PostLikes_UserId_PostId");
        builder.HasIndex(pl => pl.PostId);
    }
}

public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
        builder.HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.User)
            .WithMany()
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pc => pc.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(pc => pc.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pc => pc.PostId);
        builder.HasIndex(pc => pc.ParentCommentId);
    }
}

public class PostCommentLikeConfiguration : IEntityTypeConfiguration<PostCommentLike>
{
    public void Configure(EntityTypeBuilder<PostCommentLike> builder)
    {
        builder.HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cl => cl.PostComment)
            .WithMany(pc => pc.Likes)
            .HasForeignKey(cl => cl.PostCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cl => new { cl.UserId, cl.PostCommentId })
            .IsUnique()
            .HasDatabaseName("IX_PostCommentLikes_UserId_CommentId");
    }
}

public class PostReportConfiguration : IEntityTypeConfiguration<PostReport>
{
    public void Configure(EntityTypeBuilder<PostReport> builder)
    {
        builder.HasOne(pr => pr.Post)
            .WithMany()
            .HasForeignKey(pr => pr.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.ReportedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.ReportedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pr => pr.PostId);
        builder.HasIndex(pr => pr.IsResolved);
    }
}

public class SavedPostConfiguration : IEntityTypeConfiguration<SavedPost>
{
    public void Configure(EntityTypeBuilder<SavedPost> builder)
    {
        builder.HasKey(sp => new { sp.UserId, sp.PostId });

        builder.HasOne(sp => sp.User)
            .WithMany(u => u.SavedPosts)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Post)
            .WithMany(p => p.SavedBy)
            .HasForeignKey(sp => sp.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
