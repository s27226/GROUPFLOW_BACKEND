using GROUPFLOW.Features.Friendships.GraphQL.Responses;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Friendships.Services;

/// <summary>
/// Service interface for friendship operations - isolates business logic
/// </summary>
public interface IFriendshipService
{
    Task<IEnumerable<User>> GetUserFriendsAsync(int userId);
    Task<FriendshipStatusDto> GetFriendshipStatusAsync(int userId, int friendId);
    Task<bool> RemoveFriendshipAsync(int userId, int friendId);
    Task<Entities.Friendship> AcceptFriendRequestAsync(int userId, int friendRequestId);
}
