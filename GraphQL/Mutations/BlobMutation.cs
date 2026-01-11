using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Services;
using System.Security.Claims;

namespace NAME_WIP_BACKEND.GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class BlobMutation
    {
        /// <summary>
        /// Upload a blob file to S3 storage
        /// </summary>
        [Authorize]
        public async Task<BlobFile> UploadBlob(
            UploadBlobInput input,
            [Service] AppDbContext context,
            [Service] IS3Service s3Service,
            ClaimsPrincipal claimsPrincipal)
        {
            // Validate input using DataAnnotations
            input.ValidateInput();
            
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Parse blob type
            if (!Enum.TryParse<BlobType>(input.BlobType, true, out var blobType))
            {
                throw new ArgumentException($"Invalid blob type: {input.BlobType}");
            }

            // Decode base64 data
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(input.Base64Data);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid base64 data");
            }

            // Validate file size (25MB max)
            if (!BlobStorageHelper.ValidateFileSize(fileBytes.Length))
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {BlobStorageHelper.GetFileSizeString(BlobStorageHelper.MaxFileSize)}");
            }

            // Validate content type
            if (!BlobStorageHelper.IsAllowedFileType(input.ContentType, blobType))
            {
                throw new ArgumentException($"File type {input.ContentType} is not allowed for {blobType}");
            }

            // Authorization checks
            await ValidateUploadPermissions(context, userId, blobType, input.ProjectId, input.PostId);

            // Generate blob path
            var blobPath = BlobStorageHelper.GenerateBlobPath(
                blobType,
                userId,
                input.FileName,
                input.ProjectId,
                input.PostId
            );

            // Upload to S3
            using var stream = new MemoryStream(fileBytes);
            await s3Service.UploadFileAsync(stream, input.FileName, input.ContentType, blobPath);

            // Save to database
            var blobFile = new BlobFile
            {
                FileName = input.FileName,
                BlobPath = blobPath,
                ContentType = input.ContentType,
                FileSize = fileBytes.Length,
                Type = blobType,
                UploadedByUserId = userId,
                ProjectId = input.ProjectId,
                PostId = input.PostId,
                UploadedAt = DateTime.UtcNow
            };

            context.BlobFiles.Add(blobFile);
            await context.SaveChangesAsync();

            return blobFile;
        }

        /// <summary>
        /// Delete a blob file
        /// </summary>
        [Authorize]
        public async Task<bool> DeleteBlob(
            DeleteBlobInput input,
            [Service] AppDbContext context,
            [Service] IS3Service s3Service,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var blobFile = await context.BlobFiles
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == input.BlobId && !b.IsDeleted);

            if (blobFile == null)
            {
                throw new Exception("Blob file not found");
            }

            // Authorization checks
            var canDelete = false;

            switch (blobFile.Type)
            {
                case BlobType.UserProfilePicture:
                case BlobType.UserBanner:
                    canDelete = blobFile.UploadedByUserId == userId;
                    break;

                case BlobType.ProjectLogo:
                case BlobType.ProjectBanner:
                    if (blobFile.Project != null)
                    {
                        canDelete = blobFile.Project.OwnerId == userId;
                    }
                    break;

                case BlobType.ProjectFile:
                    if (blobFile.Project != null)
                    {
                        var isOwner = blobFile.Project.OwnerId == userId;
                        var isCollaborator = await context.UserProjects
                            .AnyAsync(up => up.ProjectId == blobFile.ProjectId && up.UserId == userId);
                        canDelete = isOwner || isCollaborator;
                    }
                    break;

                case BlobType.PostImage:
                    canDelete = blobFile.UploadedByUserId == userId;
                    break;
            }

            if (!canDelete)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this file");
            }

            // Soft delete in database
            blobFile.IsDeleted = true;
            blobFile.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Delete from S3
            await s3Service.DeleteFileAsync(blobFile.BlobPath);

            return true;
        }

        /// <summary>
        /// Update user profile picture
        /// </summary>
        [Authorize]
        public async Task<User> UpdateUserProfileImage(
            UpdateUserProfileImageInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            if (input.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own profile picture");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.ProfilePicBlobId = input.ProfilePicBlobId;

            await context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Update user banner image
        /// </summary>
        [Authorize]
        public async Task<User> UpdateUserBannerImage(
            UpdateUserBannerImageInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            if (input.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own banner");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.BannerPicBlobId = input.BannerPicBlobId;

            await context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Update project image
        /// </summary>
        [Authorize]
        public async Task<Project> UpdateProjectImage(
            UpdateProjectImageInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var project = await context.Projects.FindAsync(input.ProjectId);
            if (project == null)
            {
                throw new Exception("Project not found");
            }

            if (project.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the project owner can update the project image");
            }

            project.ImageBlobId = input.ImageBlobId;

            await context.SaveChangesAsync();
            return project;
        }

        /// <summary>
        /// Update project banner
        /// </summary>
        [Authorize]
        public async Task<Project> UpdateProjectBanner(
            UpdateProjectBannerInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var project = await context.Projects.FindAsync(input.ProjectId);
            if (project == null)
            {
                throw new Exception("Project not found");
            }

            if (project.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the project owner can update the project banner");
            }

            project.BannerBlobId = input.BannerBlobId;

            await context.SaveChangesAsync();
            return project;
        }

        private static async Task ValidateUploadPermissions(
            AppDbContext context,
            int userId,
            BlobType blobType,
            int? projectId,
            int? postId)
        {
            switch (blobType)
            {
                case BlobType.UserProfilePicture:
                case BlobType.UserBanner:
                    // User can upload their own profile/banner
                    return;

                case BlobType.ProjectLogo:
                case BlobType.ProjectBanner:
                    if (!projectId.HasValue)
                    {
                        throw new ArgumentException("Project ID is required for project images");
                    }

                    var projectForLogo = await context.Projects.FindAsync(projectId.Value);
                    if (projectForLogo == null)
                    {
                        throw new Exception("Project not found");
                    }

                    if (projectForLogo.OwnerId != userId)
                    {
                        throw new UnauthorizedAccessException("Only the project owner can upload project logo/banner");
                    }
                    break;

                case BlobType.ProjectFile:
                    if (!projectId.HasValue)
                    {
                        throw new ArgumentException("Project ID is required for project files");
                    }

                    var project = await context.Projects.FindAsync(projectId.Value);
                    if (project == null)
                    {
                        throw new Exception("Project not found");
                    }

                    var isOwner = project.OwnerId == userId;
                    var isCollaborator = await context.UserProjects
                        .AnyAsync(up => up.ProjectId == projectId.Value && up.UserId == userId);

                    if (!isOwner && !isCollaborator)
                    {
                        throw new UnauthorizedAccessException("Only project owner and collaborators can upload project files");
                    }
                    break;

                case BlobType.PostImage:
                    if (!postId.HasValue)
                    {
                        throw new ArgumentException("Post ID is required for post images");
                    }

                    var post = await context.Posts.FindAsync(postId.Value);
                    if (post == null)
                    {
                        throw new Exception("Post not found");
                    }

                    if (post.UserId != userId)
                    {
                        throw new UnauthorizedAccessException("Only the post creator can upload images to the post");
                    }
                    break;
            }
        }
    }
}
