using GROUPFLOW.Features.Chat.GraphQL.Queries;
using GROUPFLOW.Features.Friendships.GraphQL.Queries;
using GROUPFLOW.Features.Moderation.GraphQL.Queries;
using GROUPFLOW.Features.Notifications.GraphQL.Queries;
using GROUPFLOW.Features.Posts.GraphQL.Queries;
using GROUPFLOW.Features.Projects.GraphQL.Queries;
using GROUPFLOW.Features.Users.GraphQL.Queries;
using GROUPFLOW.Features.Blobs.GraphQL.Queries;

namespace GROUPFLOW;

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