using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public class UserMatchScoreWithStatusResponse
{
    public UserResponse User { get; set; } = null!;
    public double MatchScore { get; set; }
    public int CommonSkills { get; set; }
    public int CommonInterests { get; set; }
    public int CommonProjects { get; set; }
    public int RecentInteractions { get; set; }
    public bool HasPendingRequest { get; set; }
}