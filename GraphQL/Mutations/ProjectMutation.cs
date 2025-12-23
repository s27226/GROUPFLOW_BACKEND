using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectMutation
{
    public Project? CreateProject(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ProjectInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = new Project
        {
            Name = input.Name,
            Description = input.Description,
            ImageUrl = input.ImageUrl,
            IsPublic = input.IsPublic,
            OwnerId = userId,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        context.Projects.Add(project);
        context.SaveChanges();
        
        // Create a chat for the project
        var chat = new Chat
        {
            ProjectId = project.Id
        };
        context.Chats.Add(chat);
        
        // Add the owner to the chat
        var ownerUserChat = new UserChat
        {
            UserId = userId,
            ChatId = chat.Id
        };
        context.UserChats.Add(ownerUserChat);
        
        context.SaveChanges();
        
        // Load the owner to return complete object
        context.Entry(project).Reference(p => p.Owner).Load();
        
        return project;
    }

    public Project? UpdateProject(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UpdateProjectInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .FirstOrDefault(p => p.Id == input.Id);

        if (project == null)
        {
            return null;
        }

        // Check if user is the owner
        if (project.OwnerId != userId)
        {
            throw new GraphQLException("You don't have permission to edit this project");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(input.Name))
        {
            project.Name = input.Name;
        }
        
        if (!string.IsNullOrEmpty(input.Description))
        {
            project.Description = input.Description;
        }
        
        if (input.ImageUrl != null)
        {
            project.ImageUrl = input.ImageUrl;
        }
        
        if (input.IsPublic.HasValue)
        {
            project.IsPublic = input.IsPublic.Value;
        }

        project.LastUpdated = DateTime.UtcNow;

        context.SaveChanges();
        return project;
    }

    public Project? CreateProjectWithMembers(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        CreateProjectWithMembersInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = new Project
        {
            Name = input.Name,
            Description = input.Description,
            ImageUrl = input.ImageUrl,
            IsPublic = input.IsPublic,
            OwnerId = userId,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        context.Projects.Add(project);
        context.SaveChanges();

        // Create a chat for the project
        var chat = new Chat
        {
            ProjectId = project.Id
        };
        context.Chats.Add(chat);
        context.SaveChanges();

        // Add owner to chat (but NOT as a collaborator in UserProject - they're already the owner)
        var ownerUserChat = new UserChat
        {
            UserId = userId,
            ChatId = chat.Id
        };
        context.UserChats.Add(ownerUserChat);

        // Send invitations to all selected members instead of adding them directly
        foreach (var memberId in input.MemberUserIds)
        {
            // Skip if member is the owner
            if (memberId == userId) continue;

            // Create project invitation
            var invitation = new ProjectInvitation
            {
                ProjectId = project.Id,
                InvitingId = userId,
                InvitedId = memberId,
                Sent = DateTime.UtcNow,
                Expiring = DateTime.UtcNow.AddDays(7) // 7 days to accept
            };
            context.ProjectInvitations.Add(invitation);
        }

        context.SaveChanges();
        
        // Reload project with all relationships
        var completeProject = context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
                .ThenInclude(up => up.User)
            .FirstOrDefault(p => p.Id == project.Id);
        
        return completeProject;
    }

    public bool DeleteProject(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .Include(p => p.Events)
            .Include(p => p.Posts)
            .FirstOrDefault(p => p.Id == id);
        
        if (project == null)
        {
            throw new GraphQLException("Project not found");
        }

        // Check if user is the owner
        if (project.OwnerId != userId)
        {
            throw new GraphQLException("You don't have permission to delete this project");
        }

        // Delete project invitations
        var invitations = context.ProjectInvitations.Where(pi => pi.ProjectId == id);
        context.ProjectInvitations.RemoveRange(invitations);

        // Delete chat and related entries
        if (project.Chat != null)
        {
            var chatId = project.Chat.Id;
            
            var userChats = context.UserChats.Where(uc => uc.ChatId == chatId).ToList();
            
            // Delete entries and their reactions/readbys for each UserChat
            foreach (var userChat in userChats)
            {
                var entries = context.Entries.Where(e => e.UserChatId == userChat.Id).ToList();
                foreach (var entry in entries)
                {
                    var reactions = context.EntryReactions.Where(er => er.EntryId == entry.Id);
                    context.EntryReactions.RemoveRange(reactions);
                    
                    var readBys = context.ReadBys.Where(rb => rb.EntryId == entry.Id);
                    context.ReadBys.RemoveRange(readBys);
                }
                context.Entries.RemoveRange(entries);
            }
            
            context.UserChats.RemoveRange(userChats);
            
            var sharedFiles = context.SharedFiles.Where(sf => sf.ChatId == chatId);
            context.SharedFiles.RemoveRange(sharedFiles);
            
            context.Chats.Remove(project.Chat);
        }

        // Delete project events
        context.ProjectEvents.RemoveRange(project.Events);

        // Delete posts related to project
        foreach (var post in project.Posts)
        {
            var likes = context.PostLikes.Where(pl => pl.PostId == post.Id);
            context.PostLikes.RemoveRange(likes);
            
            var comments = context.PostComments.Where(pc => pc.PostId == post.Id);
            context.PostComments.RemoveRange(comments);
            
            var savedPosts = context.SavedPosts.Where(sp => sp.PostId == post.Id);
            context.SavedPosts.RemoveRange(savedPosts);
        }
        context.Posts.RemoveRange(project.Posts);

        // Delete user-project relationships
        context.UserProjects.RemoveRange(project.Collaborators);

        // Finally delete the project
        context.Projects.Remove(project);
        context.SaveChanges();
        
        return true;
    }

    public bool RemoveProjectMember(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        int projectId,
        int userId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int currentUserId = int.Parse(userIdClaim);

        var project = context.Projects
            .Include(p => p.Collaborators)
            .Include(p => p.Chat)
            .FirstOrDefault(p => p.Id == projectId);
        
        if (project == null)
        {
            throw new GraphQLException("Project not found");
        }

        // Check if current user is the owner
        if (project.OwnerId != currentUserId)
        {
            throw new GraphQLException("You don't have permission to remove members from this project");
        }

        // Prevent owner from removing themselves
        if (userId == project.OwnerId)
        {
            throw new GraphQLException("Cannot remove the project owner");
        }

        // Find the collaborator
        var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == userId);
        if (collaborator == null)
        {
            throw new GraphQLException("User is not a member of this project");
        }

        // Remove the collaborator from the project
        context.UserProjects.Remove(collaborator);

        // Remove the user from the project chat
        if (project.Chat != null)
        {
            var userChat = context.UserChats.FirstOrDefault(uc => 
                uc.UserId == userId && uc.ChatId == project.Chat.Id);
            
            if (userChat != null)
            {
                context.UserChats.Remove(userChat);
            }
        }

        context.SaveChanges();
        
        return true;
    }
}
