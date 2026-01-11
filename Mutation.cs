using GROUPFLOW.Features.Auth.GraphQL.Mutations;
using GROUPFLOW.Features.Chat.GraphQL.Mutations;
using GROUPFLOW.Features.Friendships.GraphQL.Mutations;
using GROUPFLOW.Features.Moderation.GraphQL.Mutations;
using GROUPFLOW.Features.Notifications.GraphQL.Mutations;
using GROUPFLOW.Features.Posts.GraphQL.Mutations;
using GROUPFLOW.Features.Projects.GraphQL.Mutations;
using GROUPFLOW.Features.Users.GraphQL.Mutations;

namespace GROUPFLOW;

/// <summary>
/// Root GraphQL mutation type that aggregates all domain-specific mutations.
/// Uses constructor injection to receive mutation instances from DI container.
/// </summary>
public class Mutation
{
    public AuthMutation Auth => new();
    
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
        ModerationMutation moderation)
    {
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