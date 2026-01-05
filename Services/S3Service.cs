using Amazon.S3;
using Amazon.S3.Model;

namespace NAME_WIP_BACKEND.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3Service> _logger;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"] ?? "groupflow-storage";
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType, string blobPath, CancellationToken ct = default)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = blobPath,
                    InputStream = stream,
                    ContentType = contentType
                };

                await _s3Client.PutObjectAsync(request, ct);
                
                _logger.LogInformation($"Uploaded file to S3: {blobPath}");
                return blobPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file to S3: {blobPath}");
                throw;
            }
        }

        public async Task<string> GetPresignedUrlAsync(string blobPath, int expiryInSeconds = 604800)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = blobPath,
                    Expires = DateTime.UtcNow.AddSeconds(expiryInSeconds)
                };

                return _s3Client.GetPreSignedURL(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting presigned URL for: {blobPath}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string blobPath, CancellationToken ct = default)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = blobPath
                };

                await _s3Client.DeleteObjectAsync(request, ct);
                _logger.LogInformation($"Deleted file from S3: {blobPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file from S3: {blobPath}");
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string blobPath, CancellationToken ct = default)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = blobPath
                };

                await _s3Client.GetObjectMetadataAsync(request, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(long size, string contentType)> GetFileMetadataAsync(string blobPath, CancellationToken ct = default)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = blobPath
                };

                var response = await _s3Client.GetObjectMetadataAsync(request, ct);
                return (response.ContentLength, response.Headers.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file metadata for: {blobPath}");
                throw;
            }
        }
    }
}
