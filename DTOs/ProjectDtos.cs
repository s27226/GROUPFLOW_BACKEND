namespace NAME_WIP_BACKEND.DTOs;

/// <summary>
/// DTO for project-related operations to isolate from DB model
/// </summary>
public class ProjectDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public bool IsPublic { get; set; }
    public DateTime Created { get; set; }
    public int ViewsCount { get; set; }
    public int LikesCount { get; set; }
    public int CollaboratorsCount { get; set; }
}

public class ProjectCollaboratorDto
{
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public string Role { get; set; } = "Collaborator";
    public DateTime JoinedAt { get; set; }
}
