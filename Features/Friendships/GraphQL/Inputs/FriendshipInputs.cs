using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Friendships.GraphQL.Inputs;

public record FriendRequestInput(
    [property: Range(1, int.MaxValue)]
    int RequesterId,

    [property: Range(1, int.MaxValue)]
    int RequesteeId
);

public record UpdateFriendRequestInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: Range(1, int.MaxValue)]
    int? RequesterId,

    [property: Range(1, int.MaxValue)]
    int? RequesteeId
);
