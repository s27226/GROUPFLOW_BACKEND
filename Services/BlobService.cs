using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Services;

public class BlobService
{
    private readonly AppDbContext _context;
    private readonly IS3Service _s3Service;

    public BlobService(AppDbContext context, IS3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
    }

    public async Task<BlobFile> UploadBlob(int userId, UploadBlobInput input)
    {
        // Parse blob type
        if (!Enum.TryParse<BlobType>(input.BlobType, true, out var blobType))
            throw new ArgumentException($"Invalid blob type: {input.BlobType}");

        // Decode base64
        byte[] fileBytes;
        try
        {
            fileBytes = Convert.FromBase64String(input.Base64Data);
        }
        catch
        {
            throw new ArgumentException("Invalid base64 data");
        }

        if (!BlobStorageHelper.ValidateFileSize(fileBytes.Length))
            throw new ArgumentException($"File size exceeds {BlobStorageHelper.GetFileSizeString(BlobStorageHelper.MaxFileSize)}");

        if (!BlobStorageHelper.IsAllowedFileType(input.ContentType, blobType))
            throw new ArgumentException($"File type {input.ContentType} is not allowed for {blobType}");

        await ValidateUploadPermissions(userId, blobType, input.ProjectId, input.PostId);

        var blobPath = BlobStorageHelper.GenerateBlobPath(blobType, userId, input.FileName, input.ProjectId, input.PostId);

        using var stream = new MemoryStream(fileBytes);
        await _s3Service.UploadFileAsync(stream, input.FileName, input.ContentType, blobPath);

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

        _context.BlobFiles.Add(blobFile);
        await _context.SaveChangesAsync();

        return blobFile;
    }

    public async Task<bool> DeleteBlob(int userId, int blobId)
    {
        var blobFile = await _context.BlobFiles
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.Id == blobId && !b.IsDeleted);

        if (blobFile == null) throw new GraphQLException("Blob file not found");

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
                    canDelete = blobFile.Project.OwnerId == userId;
                break;
            case BlobType.ProjectFile:
                if (blobFile.Project != null)
                {
                    var isOwner = blobFile.Project.OwnerId == userId;
                    var isCollaborator = await _context.UserProjects
                        .AnyAsync(up => up.ProjectId == blobFile.ProjectId && up.UserId == userId);
                    canDelete = isOwner || isCollaborator;
                }
                break;
            case BlobType.PostImage:
                canDelete = blobFile.UploadedByUserId == userId;
                break;
        }

        if (!canDelete) throw new UnauthorizedAccessException("No permission to delete this file");

        blobFile.IsDeleted = true;
        blobFile.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _s3Service.DeleteFileAsync(blobFile.BlobPath);
        return true;
    }

    private async Task ValidateUploadPermissions(int userId, BlobType blobType, int? projectId, int? postId)
    {
        switch (blobType)
        {
            case BlobType.UserProfilePicture:
            case BlobType.UserBanner:
                return;
            case BlobType.ProjectLogo:
            case BlobType.ProjectBanner:
                if (!projectId.HasValue) throw new ArgumentException("Project ID is required");
                var project = await _context.Projects.FindAsync(projectId.Value);
                if (project == null) throw new GraphQLException("Project not found");
                if (project.OwnerId != userId)
                    throw new UnauthorizedAccessException("Only project owner can upload this file");
                break;
            case BlobType.ProjectFile:
                if (!projectId.HasValue) throw new ArgumentException("Project ID is required");
                var proj = await _context.Projects.FindAsync(projectId.Value);
                if (proj == null) throw new GraphQLException("Project not found");
                var isOwner = proj.OwnerId == userId;
                var isCollaborator = await _context.UserProjects
                    .AnyAsync(up => up.ProjectId == projectId.Value && up.UserId == userId);
                if (!isOwner && !isCollaborator)
                    throw new UnauthorizedAccessException("Only owner or collaborators can upload project files");
                break;
            case BlobType.PostImage:
                if (!postId.HasValue) throw new ArgumentException("Post ID is required");
                var post = await _context.Posts.FindAsync(postId.Value);
                if (post == null) throw new GraphQLException("Post not found");
                if (post.UserId != userId)
                    throw new UnauthorizedAccessException("Only post creator can upload images");
                break;
        }
    }
    
    public async Task<User> UpdateUserBannerImage(int userId, UpdateUserBannerImageInput input)
    {
        if (input.UserId != userId)
            throw new UnauthorizedAccessException("You can only update your own banner");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new GraphQLException("User not found");

        user.BannerPicBlobId = input.BannerPicBlobId;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<Project> UpdateProjectImage(int userId, UpdateProjectImageInput input)
    {
        var project = await _context.Projects.FindAsync(input.ProjectId);
        if (project == null) throw new GraphQLException("Project not found");
        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the project owner can update the project image");

        project.ImageBlobId = input.ImageBlobId;
        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<Project> UpdateProjectBanner(int userId, UpdateProjectBannerInput input)
    {
        var project = await _context.Projects.FindAsync(input.ProjectId);
        if (project == null) throw new GraphQLException("Project not found");
        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the project owner can update the project banner");

        project.BannerBlobId = input.BannerBlobId;
        await _context.SaveChangesAsync();

        return project;
    }
    
}
