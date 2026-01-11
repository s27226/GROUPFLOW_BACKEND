using System.Security.Claims;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Friendships.Services;
using GROUPFLOW.Features.Friendships.DTOs;

namespace GROUPFLOW.Features.Friendships.GraphQL;

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
