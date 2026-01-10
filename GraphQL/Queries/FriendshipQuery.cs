using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services.Friendship;
using NAME_WIP_BACKEND.DTOs;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class FriendshipQuery
{
    /// <summary>
    /// Get user's friends - now uses service layer
    /// </summary>
    [GraphQLName("myfriends")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<User>> GetMyFriends(
        [Service] IFriendshipService friendshipService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return await friendshipService.GetUserFriendsAsync(userId);
    }

    /// <summary>
    /// Get friendship status - now returns DTO to isolate from DB model
    /// </summary>
    [GraphQLName("friendshipstatus")]
    public async Task<string> GetFriendshipStatus(
        [Service] IFriendshipService friendshipService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int friendId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var statusDto = await friendshipService.GetFriendshipStatusAsync(userId, friendId);
        return statusDto.Status;
    }
}