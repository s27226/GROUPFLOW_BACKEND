using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record ProjectInvitationInput(
    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, int.MaxValue)]
    int InvitingId,

    [property: Range(1, int.MaxValue)]
    int InvitedId
);
public record UpdateProjectInvitationInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: Range(1, int.MaxValue)]
    int? ProjectId,

    [property: Range(1, int.MaxValue)]
    int? InvitingId,

    [property: Range(1, int.MaxValue)]
    int? InvitedId
);
