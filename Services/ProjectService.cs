using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.GraphQL.Responses;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Services;

/// <summary>
/// Service for project-related operations.
/// Encapsulates business logic and provides transaction support.
/// </summary>
public class ProjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim))
            throw new AuthenticationException();
        return int.Parse(claim);
    }

    public async Task<Project> CreateProjectAsync(
        ClaimsPrincipal user,
        ProjectInput input,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        var project = new Project
        {
            Name = input.Name,
            Description = input.Description,
            Image = input.ImageUrl,
            IsPublic = input.IsPublic,
            OwnerId = userId,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);

        // Add skills if provided
        if (input.Skills?.Any() == true)
        {
            foreach (var skillName in input.Skills)
            {
                _context.ProjectSkills.Add(new ProjectSkill
                {
                    ProjectId = project.Id,
                    SkillName = skillName,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        // Add interests if provided
        if (input.Interests?.Any() == true)
        {
            foreach (var interestName in input.Interests)
            {
                _context.ProjectInterests.Add(new ProjectInterest
                {
                    ProjectId = project.Id,
                    InterestName = interestName,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        // Create a chat for the project
        var chat = new Chat { ProjectId = project.Id };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync(ct);

        // Add owner to chat
        _context.UserChats.Add(new UserChat { UserId = userId, ChatId = chat.Id });
        await _context.SaveChangesAsync(ct);

        await _context.Entry(project).Reference(p => p.Owner).LoadAsync(ct);
        await _context.Entry(project).Collection(p => p.Skills).LoadAsync(ct);
        await _context.Entry(project).Collection(p => p.Interests).LoadAsync(ct);

        _logger.LogInformation("User {UserId} created project {ProjectId}: {ProjectName}", userId, project.Id, project.Name);

        return project;
    }

    public async Task<Project?> UpdateProjectAsync(
        ClaimsPrincipal user,
        UpdateProjectInput input,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == input.Id, ct);

        if (project == null)
            throw EntityNotFoundException.Project(input.Id);

        if (project.OwnerId != userId)
            throw AuthorizationException.NotProjectOwner();

        if (!string.IsNullOrEmpty(input.Name))
            project.Name = input.Name;

        if (!string.IsNullOrEmpty(input.Description))
            project.Description = input.Description;

        if (input.ImageUrl != null)
            project.Image = input.ImageUrl;

        if (input.IsPublic.HasValue)
            project.IsPublic = input.IsPublic.Value;

        project.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated project {ProjectId}", userId, project.Id);

        return project;
    }

    public async Task<bool> DeleteProjectAsync(
        ClaimsPrincipal user,
        int projectId,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects.FindAsync(new object[] { projectId }, ct);

        if (project == null)
            throw EntityNotFoundException.Project(projectId);

        if (project.OwnerId != userId)
            throw AuthorizationException.NotProjectOwner();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, projectId);

        return true;
    }

    public async Task<bool> RemoveMemberAsync(
        ClaimsPrincipal user,
        int projectId,
        int memberUserId,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects.FindAsync(new object[] { projectId }, ct);
        if (project == null)
            throw EntityNotFoundException.Project(projectId);

        if (project.OwnerId != userId)
            throw AuthorizationException.NotProjectOwner();

        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.UserId == memberUserId, ct);

        if (userProject == null)
            throw new EntityNotFoundException("Project member");

        _context.UserProjects.Remove(userProject);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} removed member {MemberId} from project {ProjectId}", userId, memberUserId, projectId);

        return true;
    }

    public async Task<bool> LikeProjectAsync(
        ClaimsPrincipal user,
        int projectId,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        if (!await _context.Projects.AnyAsync(p => p.Id == projectId, ct))
            throw EntityNotFoundException.Project(projectId);

        if (await _context.ProjectLikes.AnyAsync(l => l.UserId == userId && l.ProjectId == projectId, ct))
            throw DuplicateEntityException.AlreadyLiked();

        _context.ProjectLikes.Add(new ProjectLike
        {
            UserId = userId,
            ProjectId = projectId,
            Created = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} liked project {ProjectId}", userId, projectId);

        return true;
    }

    public async Task<bool> UnlikeProjectAsync(
        ClaimsPrincipal user,
        int projectId,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        var like = await _context.ProjectLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ProjectId == projectId, ct);

        if (like == null)
            throw new BusinessRuleException(AppConstants.NotLiked);

        _context.ProjectLikes.Remove(like);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} unliked project {ProjectId}", userId, projectId);

        return true;
    }

    public async Task<bool> RecordViewAsync(
        ClaimsPrincipal user,
        int projectId,
        CancellationToken ct = default)
    {
        int userId = GetUserId(user);

        if (!await _context.Projects.AnyAsync(p => p.Id == projectId, ct))
            throw EntityNotFoundException.Project(projectId);

        // Check if user already viewed today
        var today = DateTime.UtcNow.Date;
        var existingView = await _context.ProjectViews
            .FirstOrDefaultAsync(v => v.UserId == userId && v.ProjectId == projectId && v.ViewDate.Date == today, ct);

        if (existingView == null)
        {
            _context.ProjectViews.Add(new ProjectView
            {
                UserId = userId,
                ProjectId = projectId,
                ViewDate = today,
                Created = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            _logger.LogDebug("User {UserId} viewed project {ProjectId}", userId, projectId);
        }

        return true;
    }

    public async Task<Project?> GetProjectByIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == userId)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.Created)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Project>> GetPublicProjectsAsync(int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _context.Projects
            .Where(p => p.IsPublic)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.Created)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
