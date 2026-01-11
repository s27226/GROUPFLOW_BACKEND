using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for project recommendation operations.
/// </summary>
public class ProjectRecommendationMutation
{
    private readonly ILogger<ProjectRecommendationMutation> _logger;

    public ProjectRecommendationMutation(ILogger<ProjectRecommendationMutation> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectRecommendation> CreateProjectRecommendation(
        [Service] AppDbContext context,
        ProjectRecommendationInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var rec = new ProjectRecommendation
        {
            UserId = input.UserId,
            ProjectId = input.ProjectId,
            RecValue = input.RecValue
        };

        context.ProjectRecommendations.Add(rec);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("Created recommendation for user {UserId} on project {ProjectId}", input.UserId, input.ProjectId);
        return rec;
    }

    public async Task<ProjectRecommendation?> UpdateProjectRecommendation(
        [Service] AppDbContext context,
        UpdateProjectRecommendationInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var rec = await context.ProjectRecommendations.FindAsync(new object[] { input.Id }, ct);
        if (rec == null) return null;

        if (input.UserId.HasValue) rec.UserId = input.UserId.Value;
        if (input.ProjectId.HasValue) rec.ProjectId = input.ProjectId.Value;
        if (input.RecValue.HasValue) rec.RecValue = input.RecValue.Value;

        await context.SaveChangesAsync(ct);
        return rec;
    }

    public async Task<bool> DeleteProjectRecommendation(
        [Service] AppDbContext context,
        int id,
        CancellationToken ct = default)
    {
        var rec = await context.ProjectRecommendations.FindAsync(new object[] { id }, ct);
        if (rec == null) return false;

        context.ProjectRecommendations.Remove(rec);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
