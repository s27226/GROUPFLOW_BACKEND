using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class SavedPostMutation
{
    private readonly SavedPostService _service;

    public SavedPostMutation(SavedPostService service)
    {
        _service = service;
    }

    [GraphQLName("savePost")]
    public Task<SavedPost> SavePost(
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return _service.SavePost(userId, postId);
    }

    [GraphQLName("unsavePost")]
    public Task<bool> UnsavePost(
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return _service.UnsavePost(userId, postId);
    }
}
