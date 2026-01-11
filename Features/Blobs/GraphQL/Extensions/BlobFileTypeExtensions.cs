using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Features.Blobs.Entities;
using GROUPFLOW.Features.Blobs.Services;

namespace GROUPFLOW.Features.Blobs.GraphQL.Extensions;

[ExtendObjectType(typeof(BlobFile))]
public class BlobFileTypeExtensions
{
    /// <summary>
    /// Get presigned URL for the blob file
    /// Returns a temporary presigned URL that can be used to access the file from S3
    /// </summary>
    public async Task<string?> GetUrl(
        [Parent] BlobFile blobFile,
        [Service] IS3Service s3Service,
        [Service] ILogger<BlobFileTypeExtensions> logger)
    {
        if (string.IsNullOrEmpty(blobFile.BlobPath))
        {
            return null;
        }

        try
        {
            return await s3Service.GetPresignedUrlAsync(blobFile.BlobPath);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - return null for graceful degradation
            logger.LogWarning(ex, "Error getting presigned URL for blob {BlobId}", blobFile.Id);
            return null;
        }
    }
}
