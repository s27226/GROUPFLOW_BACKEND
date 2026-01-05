using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NAME_WIP_BACKEND.Services;

public class ProjectRecommendationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectRecommendationService> _logger;

    public ProjectRecommendationService(AppDbContext context, ILogger<ProjectRecommendationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProjectRecommendation> CreateRecommendation(ProjectRecommendationInput input, CancellationToken ct = default)
    {
        var rec = new ProjectRecommendation
        {
            UserId = input.UserId,
            ProjectId = input.ProjectId,
            RecValue = input.RecValue
        };

        _context.ProjectRecommendations.Add(rec);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created recommendation {RecommendationId}: User={UserId}, Project={ProjectId}, Value={RecValue}",
            rec.Id, rec.UserId, rec.ProjectId, rec.RecValue);

        return rec;
    }

    public async Task<ProjectRecommendation?> UpdateRecommendation(UpdateProjectRecommendationInput input, CancellationToken ct = default)
    {
        var rec = await _context.ProjectRecommendations.FindAsync(new object[] { input.Id }, ct);
        if (rec == null)
        {
            _logger.LogWarning("Attempted to update non-existent recommendation {RecommendationId}", input.Id);
            return null;
        }

        if (input.UserId.HasValue) rec.UserId = input.UserId.Value;
        if (input.ProjectId.HasValue) rec.ProjectId = input.ProjectId.Value;
        if (input.RecValue.HasValue) rec.RecValue = input.RecValue.Value;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Updated recommendation {RecommendationId}: User={UserId}, Project={ProjectId}, Value={RecValue}",
            rec.Id, rec.UserId, rec.ProjectId, rec.RecValue);

        return rec;
    }

    public async Task<bool> DeleteRecommendation(int id, CancellationToken ct = default)
    {
        var rec = await _context.ProjectRecommendations.FindAsync(new object[] { id }, ct);
        if (rec == null)
        {
            _logger.LogWarning("Attempted to delete non-existent recommendation {RecommendationId}", id);
            return false;
        }

        _context.ProjectRecommendations.Remove(rec);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deleted recommendation {RecommendationId}: User={UserId}, Project={ProjectId}",
            rec.Id, rec.UserId, rec.ProjectId);

        return true;
    }
}
