using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Mutations;

namespace NAME_WIP_BACKEND;

public class Mutation
{
    public AuthMutation Auth => new();
    public UserMutation User => new();
    public GroupMutation Group => new();
    public ChatMutation Chat => new();
    public EntryMutation Entry => new();
    public EmoteMutation Emote => new();
    public FriendRequestMutation FriendRequest => new();
    public GroupInvitationMutation GroupInvitation => new();
    public EntryReactionMutation EntryReaction => new();
    public ReadByMutation ReadBy => new();
    public SharedFileMutation SharedFile => new();
    public UserRoleMutation UserRole => new();
    public UserGroupMutation UserGroup => new();
    public UserChatMutation UserChat => new();
    public FriendRecommendationMutation FriendRecommendation => new();
    public GroupRecommendationMutation GroupRecommendation => new();
}