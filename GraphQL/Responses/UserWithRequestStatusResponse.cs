using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public class UserWithRequestStatusResponse
{
    public UserResponse User { get; set; } = null!;
    public bool HasPendingRequest { get; set; }
    public bool IsFriend { get; set; }
}