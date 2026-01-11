using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Blobs.Entities;

namespace GROUPFLOW.Features.Blobs.Configurations;

public class BlobFileConfiguration : IEntityTypeConfiguration<BlobFile>
{
    public void Configure(EntityTypeBuilder<BlobFile> builder)
    {
        builder.HasOne(bf => bf.UploadedBy)
            .WithMany()
            .HasForeignKey(bf => bf.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bf => bf.Project)
            .WithMany()
            .HasForeignKey(bf => bf.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(bf => bf.Post)
            .WithMany()
            .HasForeignKey(bf => bf.PostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(bf => bf.UploadedByUserId);
        builder.HasIndex(bf => bf.ProjectId);
        builder.HasIndex(bf => bf.PostId);
        builder.HasIndex(bf => bf.Type);
        builder.HasIndex(bf => bf.IsDeleted);
    }
}
