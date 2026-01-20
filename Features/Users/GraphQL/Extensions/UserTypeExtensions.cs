using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
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
        [Service] IS3Service s3Service,
        [Service] AppDbContext context)
    {
        // If ProfilePicBlob is not loaded, fetch it from the database
        var profilePicBlob = user.ProfilePicBlob;
        if (profilePicBlob == null && user.ProfilePicBlobId.HasValue)
        {
            profilePicBlob = await context.BlobFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == user.ProfilePicBlobId.Value);
        }
        
        // If user has a blob, return presigned URL
        if (profilePicBlob != null && !string.IsNullOrEmpty(profilePicBlob.BlobPath))
        {
            try
            {
                return await s3Service.GetPresignedUrlAsync(profilePicBlob.BlobPath);
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
        [Service] IS3Service s3Service,
        [Service] AppDbContext context)
    {
        // If BannerPicBlob is not loaded, fetch it from the database
        var bannerPicBlob = user.BannerPicBlob;
        if (bannerPicBlob == null && user.BannerPicBlobId.HasValue)
        {
            bannerPicBlob = await context.BlobFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == user.BannerPicBlobId.Value);
        }
        
        // If user has a blob, return presigned URL
        if (bannerPicBlob != null && !string.IsNullOrEmpty(bannerPicBlob.BlobPath))
        {
            try
            {
                return await s3Service.GetPresignedUrlAsync(bannerPicBlob.BlobPath);
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
