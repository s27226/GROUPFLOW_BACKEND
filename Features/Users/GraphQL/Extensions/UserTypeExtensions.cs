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
    /// Returns presigned S3 URL if blob exists, otherwise dicebear identicon
    /// </summary>
    public async Task<string> GetProfilePicUrl(
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
                // Fall back to dicebear if S3 fails
                return $"https://api.dicebear.com/9.x/identicon/svg?seed={user.Nickname}";
            }
        }
        
        // Default fallback to dicebear identicon
        return $"https://api.dicebear.com/9.x/identicon/svg?seed={user.Nickname}";
    }

    /// <summary>
    /// Get presigned URL for user banner from blob storage
    /// Returns presigned S3 URL if blob exists, otherwise picsum placeholder
    /// </summary>
    public async Task<string> GetBannerPicUrl(
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
                // Fall back to picsum if S3 fails
                return $"https://picsum.photos/900/200?random={user.Id}";
            }
        }
        
        // Default fallback to picsum placeholder
        return $"https://picsum.photos/900/200?random={user.Id}";
    }
}
