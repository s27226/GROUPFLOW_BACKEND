using GroupFlow_BACKEND.Models;

namespace GroupFlow_BACKEND.Services
{
    /// <summary>
    /// Helper class for organizing blob storage paths
    /// Blob organization strategy:
    /// - user/{userId}/profile/{filename} - User profile pictures
    /// - user/{userId}/banner/{filename} - User banners
    /// - project/{projectId}/logo/{filename} - Project logos
    /// - project/{projectId}/banner/{filename} - Project banners
    /// - project/{projectId}/files/{filename} - Project files
    /// - post/{postId}/{filename} - Post images
    /// </summary>
    public static class BlobStorageHelper
    {
        private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

        public static long MaxFileSize => MaxFileSizeBytes;

        public static string GenerateBlobPath(BlobType type, int userId, string fileName, int? projectId = null, int? postId = null)
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uniqueFileName = $"{timestamp}_{sanitizedFileName}";

            return type switch
            {
                BlobType.UserProfilePicture => $"user/{userId}/profile/{uniqueFileName}",
                BlobType.UserBanner => $"user/{userId}/banner/{uniqueFileName}",
                BlobType.ProjectLogo => $"project/{projectId}/logo/{uniqueFileName}",
                BlobType.ProjectBanner => $"project/{projectId}/banner/{uniqueFileName}",
                BlobType.ProjectFile => $"project/{projectId}/files/{uniqueFileName}",
                BlobType.PostImage => $"post/{postId}/{uniqueFileName}",
                _ => throw new ArgumentException($"Unknown blob type: {type}")
            };
        }

        public static bool ValidateFileSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSizeBytes;
        }

        public static string GetFileSizeString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static bool IsImageContentType(string contentType)
        {
            var allowedImageTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/svg+xml"
            };

            return allowedImageTypes.Contains(contentType.ToLowerInvariant());
        }

        public static bool IsAllowedFileType(string contentType, BlobType type)
        {
            // For images (profile pics, banners, logos, post images), only allow image types
            if (type == BlobType.UserProfilePicture || 
                type == BlobType.UserBanner || 
                type == BlobType.ProjectLogo || 
                type == BlobType.ProjectBanner || 
                type == BlobType.PostImage)
            {
                return IsImageContentType(contentType);
            }

            // For project files, allow more types
            if (type == BlobType.ProjectFile)
            {
                var allowedTypes = new[]
                {
                    // Images
                    "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml",
                    // Documents
                    "application/pdf",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "application/vnd.ms-powerpoint",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    // Text
                    "text/plain",
                    "text/csv",
                    "text/markdown",
                    // Archives
                    "application/zip",
                    "application/x-zip-compressed",
                    "application/x-rar-compressed",
                    // Code
                    "text/html",
                    "text/css",
                    "text/javascript",
                    "application/json",
                    "application/xml"
                };

                return allowedTypes.Contains(contentType.ToLowerInvariant());
            }

            return false;
        }

        private static string SanitizeFileName(string fileName)
        {
            // Remove any path information
            fileName = System.IO.Path.GetFileName(fileName);
            
            // Remove invalid characters
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Limit length
            if (sanitized.Length > 100)
            {
                var extension = System.IO.Path.GetExtension(sanitized);
                var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(sanitized);
                sanitized = nameWithoutExtension.Substring(0, 100 - extension.Length) + extension;
            }

            return sanitized;
        }
    }
}
