using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.GraphQL.Inputs;
using GroupFlow_BACKEND.Models;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

public class ProjectRecommendationMutation
{
    public ProjectRecommendation CreateProjectRecommendation(AppDbContext context, ProjectRecommendationInput input)
    {
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
