using HotChocolate;
using HotChocolate.Types;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Types
{
    [ExtendObjectType(typeof(BlobFile))]
    public class BlobFileTypeExtensions
    {
        /// <summary>
        /// Get presigned URL for the blob file
        /// Returns a temporary presigned URL that can be used to access the file from S3
        /// </summary>
        public async Task<string?> GetUrl(
            [Parent] BlobFile blobFile,
            [Service] IS3Service s3Service)
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
                Console.WriteLine($"Error getting presigned URL for blob {blobFile.Id}: {ex.Message}");
                return null;
            }
        }
    }
}
