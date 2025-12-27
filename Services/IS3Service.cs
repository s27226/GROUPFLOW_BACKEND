namespace NAME_WIP_BACKEND.Services
{
    public interface IS3Service
    {
        /// <summary>
        /// Upload a file to S3
        /// </summary>
        /// <param name="stream">File stream to upload</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="blobPath">Path within the bucket (e.g., "user/123/profile/avatar.jpg")</param>
        /// <returns>The blob path in S3</returns>
        Task<string> UploadFileAsync(Stream stream, string fileName, string contentType, string blobPath);
        
        /// <summary>
        /// Get a presigned URL for accessing a file
        /// </summary>
        /// <param name="blobPath">Path to the blob in S3</param>
        /// <param name="expiryInSeconds">URL expiry time (default: 7 days)</param>
        /// <returns>Presigned URL</returns>
        Task<string> GetPresignedUrlAsync(string blobPath, int expiryInSeconds = 604800);
        
        /// <summary>
        /// Delete a file from S3
        /// </summary>
        /// <param name="blobPath">Path to the blob in S3</param>
        Task DeleteFileAsync(string blobPath);
        
        /// <summary>
        /// Check if a file exists in S3
        /// </summary>
        /// <param name="blobPath">Path to the blob in S3</param>
        Task<bool> FileExistsAsync(string blobPath);
        
        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="blobPath">Path to the blob in S3</param>
        Task<(long size, string contentType)> GetFileMetadataAsync(string blobPath);
    }
}
