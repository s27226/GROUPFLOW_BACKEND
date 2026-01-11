using GROUPFLOW.DTOs;
using GROUPFLOW.Models;

namespace GROUPFLOW.Services.Friendship;

/// <summary>
/// Service interface for friendship operations - isolates business logic
/// </summary>
public interface IFriendshipService
{
    Task<IEnumerable<User>> GetUserFriendsAsync(int userId);
    Task<FriendshipStatusDto> GetFriendshipStatusAsync(int userId, int friendId);
    Task<bool> RemoveFriendshipAsync(int userId, int friendId);
    Task<Models.Friendship> AcceptFriendRequestAsync(int userId, int friendRequestId);
}
