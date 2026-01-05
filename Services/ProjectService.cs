using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Services;

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
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<Project> CreateProject(
        ClaimsPrincipal user,
        ProjectInput input,
        CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
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
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {ProjectId} created by user {UserId}", project.Id, userId);

            await AddSkillsAndInterests(project.Id, input.Skills, input.Interests, cancellationToken);

            var chat = new Chat { ProjectId = project.Id };
            _context.Chats.Add(chat);

            _context.UserChats.Add(new UserChat
            {
                UserId = userId,
                ChatId = chat.Id
            });

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await LoadFullProject(project.Id, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Project?> UpdateProject(
        ClaimsPrincipal user,
        UpdateProjectInput input,
        CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == input.Id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Update failed: Project {ProjectId} not found", input.Id);
            return null;
        }

        if (project.OwnerId != userId)
            throw new GraphQLException("You don't have permission to edit this project");

        if (!string.IsNullOrWhiteSpace(input.Name))
            project.Name = input.Name;

        if (!string.IsNullOrWhiteSpace(input.Description))
            project.Description = input.Description;

        if (input.ImageUrl != null)
            project.Image = input.ImageUrl;

        if (input.IsPublic.HasValue)
            project.IsPublic = input.IsPublic.Value;

        project.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} updated by user {UserId}", project.Id, userId);
        return project;
    }

    public async Task<bool> DeleteProject(
        ClaimsPrincipal user,
        int projectId,
        CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .Include(p => p.Events)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Delete failed: Project {ProjectId} not found", projectId);
            throw new GraphQLException("Project not found");
        }

        if (project.OwnerId != userId)
            throw new GraphQLException("You don't have permission to delete this project");

        await RemoveProjectDependencies(project, cancellationToken);
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} deleted by user {UserId}", projectId, userId);
        return true;
    }

    public async Task<bool> RemoveMember(
        ClaimsPrincipal user,
        int projectId,
        int memberId,
        CancellationToken cancellationToken = default)
    {
        int ownerId = GetUserId(user);

        var project = await _context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Remove member failed: Project {ProjectId} not found", projectId);
            throw new GraphQLException("Project not found");
        }

        if (project.OwnerId != ownerId)
            throw new GraphQLException("You don't have permission");

        if (memberId == project.OwnerId)
            throw new GraphQLException("Cannot remove owner");

        var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == memberId);
        if (collaborator == null)
        {
            _logger.LogWarning("Remove member failed: User {MemberId} is not a collaborator in project {ProjectId}", memberId, projectId);
            throw new GraphQLException("User is not a collaborator");
        }

        _context.UserProjects.Remove(collaborator);

        if (project.Chat != null)
        {
            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(uc =>
                    uc.ChatId == project.Chat.Id && uc.UserId == memberId, cancellationToken);

            if (userChat != null)
                _context.UserChats.Remove(userChat);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {MemberId} removed from project {ProjectId} by owner {OwnerId}", memberId, projectId, ownerId);
        return true;
    }

    public async Task<bool> LikeProject(ClaimsPrincipal user, int projectId, CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);

        bool exists = await _context.ProjectLikes.AnyAsync(pl =>
            pl.ProjectId == projectId && pl.UserId == userId, cancellationToken);

        if (exists)
            return false;

        _context.ProjectLikes.Add(new ProjectLike
        {
            ProjectId = projectId,
            UserId = userId,
            Created = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} liked project {ProjectId}", userId, projectId);
        return true;
    }

    public async Task<bool> UnlikeProject(ClaimsPrincipal user, int projectId, CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);

        var like = await _context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.UserId == userId, cancellationToken);

        if (like == null)
            return false;

        _context.ProjectLikes.Remove(like);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} unliked project {ProjectId}", userId, projectId);
        return true;
    }

    public async Task<bool> RecordView(ClaimsPrincipal user, int projectId, CancellationToken cancellationToken = default)
    {
        int userId = GetUserId(user);
        var today = DateTime.UtcNow.Date;

        bool viewed = await _context.ProjectViews.AnyAsync(v =>
            v.ProjectId == projectId &&
            v.UserId == userId &&
            v.ViewDate == today, cancellationToken);

        if (viewed)
            return false;

        _context.ProjectViews.Add(new ProjectView
        {
            ProjectId = projectId,
            UserId = userId,
            ViewDate = today,
            Created = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} viewed project {ProjectId}", userId, projectId);
        return true;
    }

    // ================== helpers ==================

    private async Task AddSkillsAndInterests(
        int projectId,
        IEnumerable<string>? skills,
        IEnumerable<string>? interests,
        CancellationToken cancellationToken = default)
    {
        if (skills != null)
        {
            foreach (var s in skills)
                _context.ProjectSkills.Add(new ProjectSkill
                {
                    ProjectId = projectId,
                    SkillName = s,
                    AddedAt = DateTime.UtcNow
                });
        }

        if (interests != null)
        {
            foreach (var i in interests)
                _context.ProjectInterests.Add(new ProjectInterest
                {
                    ProjectId = projectId,
                    InterestName = i,
                    AddedAt = DateTime.UtcNow
                });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Project> LoadFullProject(int projectId, CancellationToken cancellationToken = default)
        => await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators).ThenInclude(c => c.User)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .FirstAsync(p => p.Id == projectId, cancellationToken);

    private async Task RemoveProjectDependencies(Project project, CancellationToken cancellationToken = default)
    {
        _context.ProjectInvitations.RemoveRange(
            _context.ProjectInvitations.Where(pi => pi.ProjectId == project.Id));

        _context.ProjectEvents.RemoveRange(project.Events);
        _context.UserProjects.RemoveRange(project.Collaborators);

        if (project.Chat != null)
            _context.Chats.Remove(project.Chat);

        await Task.CompletedTask;
    }
}
