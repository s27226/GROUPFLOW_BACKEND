using GROUPFLOW.Models;

namespace GROUPFLOW.GraphQL.Types;

public class NotificationType
{
    public int GetId([Parent] Notification notification) => notification.Id;
    public int GetUserId([Parent] Notification notification) => notification.UserId;
    public string GetType([Parent] Notification notification) => notification.Type;
    public string GetMessage([Parent] Notification notification) => notification.Message;
    public int? GetActorUserId([Parent] Notification notification) => notification.ActorUserId;
    public int? GetPostId([Parent] Notification notification) => notification.PostId;
    public bool GetIsRead([Parent] Notification notification) => notification.IsRead;
    public DateTime GetCreatedAt([Parent] Notification notification) => notification.CreatedAt;
    
    public User? GetActorUser([Parent] Notification notification) => notification.ActorUser;
    public Post? GetPost([Parent] Notification notification) => notification.Post;
}
