using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for project operations.
/// Uses async operations, transactions, proper exception handling, and logging.
/// </summary>
public class ProjectMutation
{
    private readonly ILogger<ProjectMutation> _logger;

    public ProjectMutation(ILogger<ProjectMutation> logger)
    {
        _logger = logger;
    }

    public async Task<Project> CreateProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        ProjectInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
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

            context.Projects.Add(project);
            await context.SaveChangesAsync(ct);

            if (input.Skills?.Any() == true)
            {
                foreach (var skillName in input.Skills)
                {
                    context.ProjectSkills.Add(new ProjectSkill
                    {
                        ProjectId = project.Id,
                        SkillName = skillName,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            if (input.Interests?.Any() == true)
            {
                foreach (var interestName in input.Interests)
                {
                    context.ProjectInterests.Add(new ProjectInterest
                    {
                        ProjectId = project.Id,
                        InterestName = interestName,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            var chat = new Chat { ProjectId = project.Id };
            context.Chats.Add(chat);
            await context.SaveChangesAsync(ct);

            context.UserChats.Add(new UserChat { UserId = userId, ChatId = chat.Id });
            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            await context.Entry(project).Reference(p => p.Owner).LoadAsync(ct);
            await context.Entry(project).Collection(p => p.Skills).LoadAsync(ct);
            await context.Entry(project).Collection(p => p.Interests).LoadAsync(ct);

            _logger.LogInformation("User {UserId} created project {ProjectId}: {ProjectName}", userId, project.Id, project.Name);
            return project;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to create project for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Project> UpdateProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        UpdateProjectInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var project = await context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == input.Id, ct)
            ?? throw EntityNotFoundException.Project(input.Id);

        if (project.OwnerId != userId)
            throw AuthorizationException.NotProjectOwner();

        if (!string.IsNullOrEmpty(input.Name)) project.Name = input.Name;
        if (!string.IsNullOrEmpty(input.Description)) project.Description = input.Description;
        if (input.ImageUrl != null) project.Image = input.ImageUrl;
        if (input.IsPublic.HasValue) project.IsPublic = input.IsPublic.Value;

        project.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated project {ProjectId}", userId, project.Id);
        return project;
    }

    public async Task<Project> CreateProjectWithMembers(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        CreateProjectWithMembersInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
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

            context.Projects.Add(project);
            await context.SaveChangesAsync(ct);

            if (input.Skills?.Any() == true)
            {
                foreach (var skillName in input.Skills)
                {
                    context.ProjectSkills.Add(new ProjectSkill
                    {
                        ProjectId = project.Id,
                        SkillName = skillName,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            if (input.Interests?.Any() == true)
            {
                foreach (var interestName in input.Interests)
                {
                    context.ProjectInterests.Add(new ProjectInterest
                    {
                        ProjectId = project.Id,
                        InterestName = interestName,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            var chat = new Chat { ProjectId = project.Id };
            context.Chats.Add(chat);
            await context.SaveChangesAsync(ct);

            context.UserChats.Add(new UserChat { UserId = userId, ChatId = chat.Id });

            foreach (var memberId in input.MemberUserIds)
            {
                if (memberId == userId) continue;

                context.ProjectInvitations.Add(new ProjectInvitation
                {
                    ProjectId = project.Id,
                    InvitingId = userId,
                    InvitedId = memberId,
                    Sent = DateTime.UtcNow,
                    Expiring = DateTime.UtcNow.AddDays(7)
                });
            }

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            var completeProject = await context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Collaborators).ThenInclude(up => up.User)
                .Include(p => p.Skills)
                .Include(p => p.Interests)
                .FirstAsync(p => p.Id == project.Id, ct);

            _logger.LogInformation("User {UserId} created project {ProjectId} with member invitations", userId, project.Id);
            return completeProject;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to create project with members for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var project = await context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .Include(p => p.Events)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw EntityNotFoundException.Project(id);

        if (project.OwnerId != userId)
            throw AuthorizationException.NotProjectOwner();

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            context.ProjectInvitations.RemoveRange(context.ProjectInvitations.Where(pi => pi.ProjectId == id));

            if (project.Chat != null)
            {
                var chatId = project.Chat.Id;
                var userChats = await context.UserChats.Where(uc => uc.ChatId == chatId).ToListAsync(ct);

                foreach (var userChat in userChats)
                {
                    var entries = await context.Entries.Where(e => e.UserChatId == userChat.Id).ToListAsync(ct);
                    foreach (var entry in entries)
                    {
                        context.EntryReactions.RemoveRange(context.EntryReactions.Where(er => er.EntryId == entry.Id));
                        context.ReadBys.RemoveRange(context.ReadBys.Where(rb => rb.EntryId == entry.Id));
                    }
                    context.Entries.RemoveRange(entries);
                }

                context.UserChats.RemoveRange(userChats);
                context.SharedFiles.RemoveRange(context.SharedFiles.Where(sf => sf.ChatId == chatId));
                context.Chats.Remove(project.Chat);
            }

            context.ProjectEvents.RemoveRange(project.Events);

            foreach (var post in project.Posts)
            {
                context.PostLikes.RemoveRange(context.PostLikes.Where(pl => pl.PostId == post.Id));
                context.PostComments.RemoveRange(context.PostComments.Where(pc => pc.PostId == post.Id));
                context.SavedPosts.RemoveRange(context.SavedPosts.Where(sp => sp.PostId == post.Id));
            }
            context.Posts.RemoveRange(project.Posts);

            context.UserProjects.RemoveRange(project.Collaborators);
            context.Projects.Remove(project);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, id);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to delete project {ProjectId}", id);
            throw;
        }
    }

    public async Task<bool> RemoveProjectMember(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int projectId,
        int userId,
        CancellationToken ct = default)
    {
        var currentUserId = httpContextAccessor.GetAuthenticatedUserId();

        var project = await context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct)
            ?? throw EntityNotFoundException.Project(projectId);

        if (project.OwnerId != currentUserId)
            throw AuthorizationException.NotProjectOwner();

        if (userId == project.OwnerId)
            throw new BusinessRuleException("Cannot remove the project owner");

        var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == userId)
            ?? throw new BusinessRuleException("User is not a member of this project");

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            context.UserProjects.Remove(collaborator);

            if (project.Chat != null)
            {
                var userChat = await context.UserChats
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChatId == project.Chat.Id, ct);
                if (userChat != null)
                    context.UserChats.Remove(userChat);
            }

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("User {CurrentUserId} removed member {UserId} from project {ProjectId}", currentUserId, userId, projectId);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    [GraphQLName("likeproject")]
    public async Task<bool> LikeProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int projectId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        _ = await context.Projects.FindAsync(new object[] { projectId }, ct)
            ?? throw EntityNotFoundException.Project(projectId);

        var existingLike = await context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.UserId == userId, ct);

        if (existingLike != null)
            return false;

        context.ProjectLikes.Add(new ProjectLike
        {
            ProjectId = projectId,
            UserId = userId,
            Created = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return true;
    }

    [GraphQLName("unlikeproject")]
    public async Task<bool> UnlikeProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int projectId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var like = await context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.UserId == userId, ct);

        if (like == null)
            return false;

        context.ProjectLikes.Remove(like);
        await context.SaveChangesAsync(ct);
        return true;
    }

    [GraphQLName("recordprojectview")]
    public async Task<bool> RecordProjectView(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int projectId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        _ = await context.Projects.FindAsync(new object[] { projectId }, ct)
            ?? throw EntityNotFoundException.Project(projectId);

        var today = DateTime.UtcNow.Date;

        var existingView = await context.ProjectViews
            .FirstOrDefaultAsync(pv => pv.ProjectId == projectId && pv.UserId == userId && pv.ViewDate == today, ct);

        if (existingView != null)
            return false;

        context.ProjectViews.Add(new ProjectView
        {
            ProjectId = projectId,
            UserId = userId,
            ViewDate = today,
            Created = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return true;
    }
}
