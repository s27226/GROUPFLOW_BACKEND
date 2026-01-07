using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record ProjectResponse(
    int Id,
    string Name,
    string Description,
    string? Image,
    string? Banner,
    DateTime Created,
    DateTime LastUpdated,
    bool IsPublic,
    UserResponse Owner,
    int CollaboratorsCount,
    int LikesCount,
    int ViewsCount,
    List<ProjectSkillResponse> Skills,
    List<ProjectInterestResponse> Interests
)
{
    public static ProjectResponse FromProject(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.Image,
            project.Banner,
            project.Created,
            project.LastUpdated,
            project.IsPublic,
            UserResponse.FromUser(project.Owner),
            project.Collaborators.Count,
            project.Likes.Count,
            project.Views.Count,
            project.Skills.Select(s => new ProjectSkillResponse(s.SkillName)).ToList(),
            project.Interests.Select(i => new ProjectInterestResponse(i.InterestName)).ToList()
        );
    }
}

public record ProjectSkillResponse(
    string SkillName
);

public record ProjectInterestResponse(
    string InterestName
);