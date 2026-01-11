namespace GROUPFLOW.Features.Friendships.GraphQL.Responses;

/// <summary>
/// DTO for friendship status between two users
/// </summary>
public class FriendshipStatusDto
{
    public string Status { get; set; } = "none";
    public bool CanSendRequest { get; set; }
    public bool CanAcceptRequest { get; set; }
}
