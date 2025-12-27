using NAME_WIP_BACKEND.GraphQL.Queries;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND;

public class Query
{
    public ChatQuery Chat => new();
    
    public EntryQuery Entry => new();
    
    
    public FriendRequestQuery FriendRequest => new();
    public ProjectInvitationQuery ProjectInvitation => new();
    public ProjectRecommendationQuery ProjectRecommendation => new();
    
    public UserChatQuery UserChat => new();
    
    public UsersQuery Users => new();
    public PostQuery Post => new();
    public ProjectQuery Project => new();
    public FriendshipQuery Friendship => new();
    public ProjectEventQuery ProjectEvent => new();
    public SavedPostQuery SavedPost => new();
    public UserTagQuery UserTag => new();
    public NotificationQuery Notification => new();
    public BlockedUserQuery BlockedUser => new();
    public AdminQuery Admin => new();
    public ModerationQuery Moderation => new();



}