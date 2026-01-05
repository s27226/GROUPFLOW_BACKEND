using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectRecommendationMutation
{
    private readonly ProjectRecommendationService _service;

    public ProjectRecommendationMutation(ProjectRecommendationService service)
    {
        _service = service;
    }

    public Task<ProjectRecommendation> CreateProjectRecommendation(ProjectRecommendationInput input)
        => _service.CreateRecommendation(input);

    public Task<ProjectRecommendation?> UpdateProjectRecommendation(UpdateProjectRecommendationInput input)
        => _service.UpdateRecommendation(input);

    public Task<bool> DeleteProjectRecommendation(int id)
        => _service.DeleteRecommendation(id);
}
