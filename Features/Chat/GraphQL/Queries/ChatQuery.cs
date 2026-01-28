using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Features.Chat.Entities;

namespace GROUPFLOW.Features.Chat.GraphQL.Queries;

public class ChatQuery
{
    [GraphQLName("allchats")]
    public async Task<List<Entities.Chat>> GetChats(AppDbContext context) 
    {
        return await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .ToListAsync();
    }
    
    [GraphQLName("chatbyid")]
    public async Task<Entities.Chat?> GetChatById(AppDbContext context, int id) 
    {
        return await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    /// <summary>
    /// Get or create a direct message chat between the current user and a friend.
    /// Direct message chats have ProjectId = null.
    /// </summary>
    [GraphQLName("getorcreatedirectchat")]
    public async Task<Entities.Chat?> GetOrCreateDirectChat(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int friendId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if users are friends
        var areFriends = await context.Friendships
            .AnyAsync(f => 
                ((f.UserId == userId && f.FriendId == friendId) ||
                 (f.FriendId == userId && f.UserId == friendId)) &&
                f.IsAccepted);
        
        if (!areFriends)
        {
            throw BusinessRuleException.UsersNotFriends();
        }
        
        // Find existing direct chat between these two users
        // Direct chats have ProjectId = null
        var existingChat = await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Where(c => c.ProjectId == null) // Direct message chats
            .Where(c => c.UserChats.Count == 2)
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId) &&
                       c.UserChats.Any(uc => uc.UserId == friendId))
            .FirstOrDefaultAsync();
        
        if (existingChat != null)
        {
            return existingChat;
        }
        
        // Create new direct message chat
        var newChat = new Entities.Chat
        {
            ProjectId = null // null indicates a direct message chat
        };
        
        context.Chats.Add(newChat);
        await context.SaveChangesAsync();
        
        // Add both users to the chat
        var userChat1 = new UserChat
        {
            ChatId = newChat.Id,
            UserId = userId
        };
        
        var userChat2 = new UserChat
        {
            ChatId = newChat.Id,
            UserId = friendId
        };
        
        context.UserChats.Add(userChat1);
        context.UserChats.Add(userChat2);
        await context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .FirstAsync(c => c.Id == newChat.Id);
    }
    
    /// <summary>
    /// Get all direct message chats for the current user
    /// </summary>
    [GraphQLName("mydirectchats")]
    public async Task<List<Entities.Chat>> GetMyDirectChats(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Where(c => c.ProjectId == null) // Direct message chats only
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId))
            .ToListAsync();
    }
}
