using GroupFlow_BACKEND.Controllers;
using GroupFlow_BACKEND.GraphQL.Mutations;

namespace GroupFlow_BACKEND;

public class Mutation
{
    public AuthMutation Auth => new();
    
    public ProjectMutation Project => new();
    
    public EntryMutation Entry => new();
    
    public FriendRequestMutation FriendRequest => new();
    public FriendshipMutation Friendship => new();
    public ProjectInvitationMutation ProjectInvitation => new();
    
    
    public ProjectRecommendationMutation ProjectRecommendation => new();
    public ProjectEventMutation ProjectEvent => new();
    public SavedPostMutation SavedPost => new();
    public UserTagMutation UserTag => new();
    public PostMutation Post => new();
    public NotificationMutation Notification => new();
    public BlockedUserMutation BlockedUser => new();
    public ModerationMutation Moderation => new();
}