using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectMutation
{
    private readonly ProjectService _service;

    public ProjectMutation(ProjectService service)
    {
        _service = service;
    }

    public Task<Project> CreateProject(
        ProjectInput input,
        ClaimsPrincipal user)
        => _service.CreateProject(user, input);

    public Task<Project?> UpdateProject(
        UpdateProjectInput input,
        ClaimsPrincipal user)
        => _service.UpdateProject(user, input);

    public Task<bool> DeleteProject(
        int id,
        ClaimsPrincipal user)
        => _service.DeleteProject(user, id);

    public Task<bool> RemoveProjectMember(
        int projectId,
        int userId,
        ClaimsPrincipal user)
        => _service.RemoveMember(user, projectId, userId);

    public Task<bool> LikeProject(
        int projectId,
        ClaimsPrincipal user)
        => _service.LikeProject(user, projectId);

    public Task<bool> UnlikeProject(
        int projectId,
        ClaimsPrincipal user)
        => _service.UnlikeProject(user, projectId);

    public Task<bool> RecordProjectView(
        int projectId,
        ClaimsPrincipal user)
        => _service.RecordView(user, projectId);
}
