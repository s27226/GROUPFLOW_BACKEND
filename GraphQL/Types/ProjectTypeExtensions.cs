using HotChocolate;
using HotChocolate.Types;
using GROUPFLOW.Models;
using GROUPFLOW.Services;

namespace GROUPFLOW.GraphQL.Types
{
    [ExtendObjectType(typeof(Project))]
    public class ProjectTypeExtensions
    {
        /// <summary>
        /// Get presigned URL for project image/logo from blob storage
        /// If blob exists, returns presigned URL; otherwise returns ImageUrl field value
        /// </summary>
        public async Task<string?> GetImageUrl(
            [Parent] Project project,
            [Service] IS3Service s3Service)
        {
            // If project has a blob, return presigned URL
            if (project.ImageBlob != null && !string.IsNullOrEmpty(project.ImageBlob.BlobPath))
            {
                try
                {
                    return await s3Service.GetPresignedUrlAsync(project.ImageBlob.BlobPath);
                }
                catch
                {
                    // Fall back to Image string if blob fails
                    return project.Image;
                }
            }
            
            // Otherwise return the Image string (URL or null)
            return project.Image;
        }

        /// <summary>
        /// Get presigned URL for project banner image
        /// </summary>
        public async Task<string?> GetBannerUrl(
            [Parent] Project project,
            [Service] IS3Service s3Service)
        {
            // If project has a blob, return presigned URL
            if (project.BannerBlob != null && !string.IsNullOrEmpty(project.BannerBlob.BlobPath))
            {
                try
                {
                    return await s3Service.GetPresignedUrlAsync(project.BannerBlob.BlobPath);
                }
                catch
                {
                    // Fall back to Banner string if blob fails
                    return project.Banner;
                }
            }
            
            // Otherwise return the Banner string (URL or null)
            return project.Banner;
        }
    }
}
