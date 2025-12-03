using NAME_WIP_BACKEND.GraphQL.Queries;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND;

public class Query
{
    public ChatQuery Chat => new();
    public EmoteQuery Emote => new();
    public EntryQuery Entry => new();
    public EntryReactionQuery EntryReaction => new();
    public FriendRecommendationQuery FriendRecommendation => new FriendRecommendationQuery();
    
    public FriendRequestQuery FriendRequest => new();
    public GroupInvitationQuery GroupInvitation => new();
    public GroupQuery Group => new();
    public GroupRecommendationQuery GroupRecommendation => new();
    public ReadByQuery ReadBy => new();
    
    public SharedFileQuery SharedFile => new();
    public UserChatQuery UserChat => new();
    public UserGroupQuery UserGroup => new();
    public UserRoleQuery UserRole => new();
    public UsersQuery Users => new();
    public PostQuery Post => new();
    public ProjectQuery Project => new();
    public FriendshipQuery Friendship => new();




}