using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectEventMutation
{
    private readonly ProjectEventService _service;

    public ProjectEventMutation(ProjectEventService service)
    {
        _service = service;
    }

    public Task<ProjectEvent> CreateProjectEvent(
        ProjectEventInput input,
        ClaimsPrincipal user)
        => _service.CreateProjectEvent(user, input);

    public Task<ProjectEvent> UpdateProjectEvent(
        UpdateProjectEventInput input,
        ClaimsPrincipal user)
        => _service.UpdateProjectEvent(user, input);

    public Task<bool> DeleteProjectEvent(
        int id,
        ClaimsPrincipal user)
        => _service.DeleteProjectEvent(user, id);
}
