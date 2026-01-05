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
        private readonly BlobService _service;

        public BlobMutation(BlobService service)
        {
            _service = service;
        }
        /// <summary>
        /// Upload a blob file to S3 storage
        /// </summary>
        [Authorize]
        public Task<BlobFile> UploadBlob(UploadBlobInput input, ClaimsPrincipal claimsPrincipal)
        {
            int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _service.UploadBlob(userId, input);
        }

        /// <summary>
        /// Delete a blob file
        /// </summary>
        [Authorize]
        public Task<bool> DeleteBlob(DeleteBlobInput input, ClaimsPrincipal claimsPrincipal)
        {
            int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _service.DeleteBlob(userId, input.BlobId);
        }

        /// <summary>
        /// Update user banner image
        /// </summary>
        [Authorize]
        public Task<User> UpdateUserBannerImage(UpdateUserBannerImageInput input, ClaimsPrincipal claimsPrincipal)
        {
            int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _service.UpdateUserBannerImage(userId, input);
        }

        /// <summary>
        /// Update project image
        /// </summary>
        [Authorize]
        public Task<Project> UpdateProjectImage(UpdateProjectImageInput input, ClaimsPrincipal claimsPrincipal)
        {
            int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _service.UpdateProjectImage(userId, input);
        }

        /// <summary>
        /// Update project banner
        /// </summary>
        [Authorize]
        public Task<Project> UpdateProjectBanner(UpdateProjectBannerInput input, ClaimsPrincipal claimsPrincipal)
        {
            int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _service.UpdateProjectBanner(userId, input);
        }

        
    }
}



