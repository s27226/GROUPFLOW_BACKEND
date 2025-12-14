namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ProjectInvitationInput(int ProjectId, int InvitingId, int InvitedId);
public record UpdateProjectInvitationInput(int Id, int? ProjectId, int? InvitingId, int? InvitedId);
