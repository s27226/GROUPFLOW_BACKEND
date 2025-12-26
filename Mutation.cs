using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Mutations;

namespace NAME_WIP_BACKEND;

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