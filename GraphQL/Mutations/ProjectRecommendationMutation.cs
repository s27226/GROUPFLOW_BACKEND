using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectRecommendationMutation
{
    public ProjectRecommendation CreateProjectRecommendation(AppDbContext context, ProjectRecommendationInput input)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var rec = new ProjectRecommendation
        {
            UserId = input.UserId,
            ProjectId = input.ProjectId,
            RecValue = input.RecValue
        };
        context.ProjectRecommendations.Add(rec);
        context.SaveChanges();
        return rec;
    }

    public ProjectRecommendation? UpdateProjectRecommendation(AppDbContext context, UpdateProjectRecommendationInput input)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var rec = context.ProjectRecommendations.Find(input.Id);
        if (rec == null) return null;
        if (input.UserId.HasValue) rec.UserId = input.UserId.Value;
        if (input.ProjectId.HasValue) rec.ProjectId = input.ProjectId.Value;
        if (input.RecValue.HasValue) rec.RecValue = input.RecValue.Value;
        context.SaveChanges();
        return rec;
    }

    public bool DeleteProjectRecommendation(AppDbContext context, int id)
    {
        var rec = context.ProjectRecommendations.Find(id);
        if (rec == null) return false;
        context.ProjectRecommendations.Remove(rec);
        context.SaveChanges();
        return true;
    }
}
