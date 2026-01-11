using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs
{
    public class UploadBlobInput
    {
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = null!;
        [Required]
        public string Base64Data { get; set; } = null!; // Base64 encoded file data
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = null!;
        [Required]
        [StringLength(50)]
        public string BlobType { get; set; } = null!; // UserProfilePicture, UserBanner, ProjectLogo, ProjectBanner, ProjectFile, PostImage
        
        [Range(1, int.MaxValue)]
        public int? ProjectId { get; set; }
        [Range(1, int.MaxValue)]
        public int? PostId { get; set; }
    }

    public class DeleteBlobInput
    {
        [Range(1, int.MaxValue)]
        public int BlobId { get; set; }
    }

    public class UpdateUserProfileImageInput
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        [Range(1, int.MaxValue)]
        public int? ProfilePicBlobId { get; set; }
    }

    public class UpdateUserBannerImageInput
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        [Range(1, int.MaxValue)]
        public int? BannerPicBlobId { get; set; }
    }

    public class UpdateProjectImageInput
    {
        public int ProjectId { get; set; }
        public int? ImageBlobId { get; set; }
    }

    public class UpdateProjectBannerInput
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
        [Range(1, int.MaxValue)]
        public int? BannerBlobId { get; set; }
    }
}
