namespace NAME_WIP_BACKEND.GraphQL.Inputs
{
    public class UploadBlobInput
    {
        public string FileName { get; set; } = null!;
        public string Base64Data { get; set; } = null!; // Base64 encoded file data
        public string ContentType { get; set; } = null!;
        public string BlobType { get; set; } = null!; // UserProfilePicture, UserBanner, ProjectLogo, ProjectBanner, ProjectFile, PostImage
        
        public int? ProjectId { get; set; }
        public int? PostId { get; set; }
    }

    public class DeleteBlobInput
    {
        public int BlobId { get; set; }
    }

    public class UpdateUserProfileImageInput
    {
        public int UserId { get; set; }
        public int? ProfilePicBlobId { get; set; }
    }

    public class UpdateUserBannerImageInput
    {
        public int UserId { get; set; }
        public int? BannerPicBlobId { get; set; }
    }

    public class UpdateProjectImageInput
    {
        public int ProjectId { get; set; }
        public int? ImageBlobId { get; set; }
    }

    public class UpdateProjectBannerInput
    {
        public int ProjectId { get; set; }
        public int? BannerBlobId { get; set; }
    }
}
