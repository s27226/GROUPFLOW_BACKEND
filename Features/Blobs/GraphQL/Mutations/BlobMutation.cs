using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Blobs.Entities;
using GROUPFLOW.Features.Blobs.GraphQL.Inputs;
using GROUPFLOW.Features.Blobs.Services;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Projects.Entities;
using System.Security.Claims;

namespace GROUPFLOW.Features.Blobs.GraphQL.Mutations;

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
            throw new AuthenticationException();
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw EntityNotFoundException.User(userId);
        }

        // Parse blob type - map frontend types to backend enum
        var mappedBlobType = input.BlobType.ToLowerInvariant() switch
        {
            "profile" => "UserProfilePicture",
            "post" => "PostImage",
            "project" => "ProjectFile",
            "chat" => "PostImage", // Chat attachments use PostImage type
            "projectlogo" => "ProjectLogo",
            "projectbanner" => "ProjectBanner",
            _ => input.BlobType
        };
        
        if (!Enum.TryParse<BlobType>(mappedBlobType, true, out var blobType))
        {
            throw ValidationException.InvalidBlobType(input.BlobType);
        }

        // Decode base64 data
        byte[] fileBytes;
        try
        {
            fileBytes = Convert.FromBase64String(input.Base64Data);
        }
        catch (Exception)
        {
            throw ValidationException.InvalidBase64Data();
        }

        // Validate file size (25MB max)
        if (!BlobStorageHelper.ValidateFileSize(fileBytes.Length))
        {
            throw ValidationException.FileSizeExceeded();
        }

        // Validate content type
        if (!BlobStorageHelper.IsAllowedFileType(input.ContentType, blobType))
        {
            throw ValidationException.FileTypeNotAllowed();
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
            throw new AuthenticationException();
        }

        var blobFile = await context.BlobFiles
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.Id == input.BlobId && !b.IsDeleted);

        if (blobFile == null)
        {
            throw EntityNotFoundException.BlobFile(input.BlobId);
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
            throw AuthorizationException.CannotDeleteFile();
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
            throw new AuthenticationException();
        }

        if (input.UserId != userId)
        {
            throw AuthorizationException.CannotUpdateProfilePicture();
        }

        var user = await context.Users
            .Include(u => u.ProfilePicBlob)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null)
        {
            throw EntityNotFoundException.User(userId);
        }

        user.ProfilePicBlobId = input.ProfilePicBlobId;

        await context.SaveChangesAsync();
        
        // Reload with blob to ensure profilePicUrl resolver works
        await context.Entry(user).Reference(u => u.ProfilePicBlob).LoadAsync();
        
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
            throw new AuthenticationException();
        }

        if (input.UserId != userId)
        {
            throw AuthorizationException.CannotUpdateBanner();
        }

        var user = await context.Users
            .Include(u => u.BannerPicBlob)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null)
        {
            throw EntityNotFoundException.User(userId);
        }

        user.BannerPicBlobId = input.BannerPicBlobId;

        await context.SaveChangesAsync();
        
        // Reload with blob to ensure bannerPicUrl resolver works
        await context.Entry(user).Reference(u => u.BannerPicBlob).LoadAsync();
        
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
            throw new AuthenticationException();
        }

        var project = await context.Projects.FindAsync(input.ProjectId);
        if (project == null)
        {
            throw EntityNotFoundException.Project(input.ProjectId);
        }

        if (project.OwnerId != userId)
        {
            throw AuthorizationException.CannotUpdateProjectImage();
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
            throw new AuthenticationException();
        }

        var project = await context.Projects.FindAsync(input.ProjectId);
        if (project == null)
        {
            throw EntityNotFoundException.Project(input.ProjectId);
        }

        if (project.OwnerId != userId)
        {
            throw AuthorizationException.CannotUpdateProjectBanner();
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
                    throw new ValidationException("projectId", "errors.PROJECT_ID_REQUIRED");
                }

                var projectForLogo = await context.Projects.FindAsync(projectId.Value);
                if (projectForLogo == null)
                {
                    throw EntityNotFoundException.Project(projectId.Value);
                }

                if (projectForLogo.OwnerId != userId)
                {
                    throw AuthorizationException.CannotUploadProjectMedia();
                }
                break;

            case BlobType.ProjectFile:
                if (!projectId.HasValue)
                {
                    throw new ValidationException("projectId", "errors.PROJECT_ID_REQUIRED");
                }

                var project = await context.Projects.FindAsync(projectId.Value);
                if (project == null)
                {
                    throw EntityNotFoundException.Project(projectId.Value);
                }

                var isOwner = project.OwnerId == userId;
                var isCollaborator = await context.UserProjects
                    .AnyAsync(up => up.ProjectId == projectId.Value && up.UserId == userId);

                if (!isOwner && !isCollaborator)
                {
                    throw AuthorizationException.CannotUploadProjectFiles();
                }
                break;

            case BlobType.PostImage:
                if (!postId.HasValue)
                {
                    throw new ValidationException("postId", "errors.POST_ID_REQUIRED");
                }

                var post = await context.Posts.FindAsync(postId.Value);
                if (post == null)
                {
                    throw EntityNotFoundException.Post(postId.Value);
                }

                if (post.UserId != userId)
                {
                    throw AuthorizationException.CannotUploadPostImages();
                }
                break;
        }
    }
}
