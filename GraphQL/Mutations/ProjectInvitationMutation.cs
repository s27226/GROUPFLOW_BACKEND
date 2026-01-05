using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectInvitationMutation
{
    private readonly ProjectInvitationService _service;

    public ProjectInvitationMutation(ProjectInvitationService service)
    {
        _service = service;
    }

    public Task<ProjectInvitation> CreateProjectInvitation(
        ProjectInvitationInput input,
        ClaimsPrincipal user)
        => _service.CreateInvitation(user, input);

    public Task<bool> AcceptProjectInvitation(
        int invitationId,
        ClaimsPrincipal user)
        => _service.AcceptInvitation(user, invitationId);

    public Task<bool> RejectProjectInvitation(
        int invitationId,
        ClaimsPrincipal user)
        => _service.RejectInvitation(user, invitationId);

    public Task<bool> DeleteProjectInvitation(int id)
        => _service.DeleteInvitation(id);

    public Task<ProjectInvitation> UpdateProjectInvitation(
        UpdateProjectInvitationInput input)
        => _service.UpdateInvitation(input);
}
