using HotChocolate;
using HotChocolate.Types;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Blobs.Services;

namespace GROUPFLOW.Features.Users.GraphQL.Extensions;

[ExtendObjectType(typeof(User))]
public class UserTypeExtensions
{
    /// <summary>
    /// Get presigned URL for user profile picture from blob storage
    /// If blob exists, returns presigned URL; otherwise returns ProfilePic field value
    /// </summary>
    public async Task<string?> GetProfilePicUrl(
        [Parent] User user,
        [Service] IS3Service s3Service)
    {
        // If user has a blob, return presigned URL
        if (user.ProfilePicBlob != null && !string.IsNullOrEmpty(user.ProfilePicBlob.BlobPath))
        {
            try
            {
                return await s3Service.GetPresignedUrlAsync(user.ProfilePicBlob.BlobPath);
            }
            catch
            {
                // Fall back to ProfilePic string if blob fails
                return user.ProfilePic;
            }
        }
        
        // Otherwise return the ProfilePic string (URL or null)
        return user.ProfilePic;
    }

    /// <summary>
    /// Get presigned URL for user banner from blob storage
    /// If blob exists, returns presigned URL; otherwise returns BannerPic field value
    /// </summary>
    public async Task<string?> GetBannerPicUrl(
        [Parent] User user,
        [Service] IS3Service s3Service)
    {
        // If user has a blob, return presigned URL
        if (user.BannerPicBlob != null && !string.IsNullOrEmpty(user.BannerPicBlob.BlobPath))
        {
            try
            {
                return await s3Service.GetPresignedUrlAsync(user.BannerPicBlob.BlobPath);
            }
            catch
            {
                // Fall back to BannerPic string if blob fails
                return user.BannerPic;
            }
        }
        
        // Otherwise return the BannerPic string (URL or null)
        return user.BannerPic;
    }
}
