using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Mutations;

namespace NAME_WIP_BACKEND;

public class Mutation
{
    public AuthMutation Auth { get; }
    public ProjectMutation Project { get; }
    public EntryMutation Entry { get; }
    public FriendRequestMutation FriendRequest { get; }
    public FriendshipMutation Friendship { get; }
    public ProjectInvitationMutation ProjectInvitation { get; }
    public ProjectRecommendationMutation ProjectRecommendation { get; }
    public ProjectEventMutation ProjectEvent { get; }
    public SavedPostMutation SavedPost { get; }
    public UserTagMutation UserTag { get; }
    public PostMutation Post { get; }
    public NotificationMutation Notification { get; }
    public BlockedUserMutation BlockedUser { get; }
    public ModerationMutation Moderation { get; }

    public Mutation(
        AuthMutation auth,
        ProjectMutation project,
        EntryMutation entry,
        FriendRequestMutation friendRequest,
        FriendshipMutation friendship,
        ProjectInvitationMutation projectInvitation,
        ProjectRecommendationMutation projectRecommendation,
        ProjectEventMutation projectEvent,
        SavedPostMutation savedPost,
        UserTagMutation userTag,
        PostMutation post,
        NotificationMutation notification,
        BlockedUserMutation blockedUser,
        ModerationMutation moderation
    )
    {
        Auth = auth;
        Project = project;
        Entry = entry;
        FriendRequest = friendRequest;
        Friendship = friendship;
        ProjectInvitation = projectInvitation;
        ProjectRecommendation = projectRecommendation;
        ProjectEvent = projectEvent;
        SavedPost = savedPost;
        UserTag = userTag;
        Post = post;
        Notification = notification;
        BlockedUser = blockedUser;
        Moderation = moderation;
    }
}