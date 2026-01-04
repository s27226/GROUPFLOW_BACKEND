using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Queries;

public class ChatQuery
{
    [GraphQLName("allchats")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Chat> GetChats(AppDbContext context) => context.Chats;
    
    [GraphQLName("chatbyid")]
    [UseProjection]
    public IQueryable<Chat> GetChatById(AppDbContext context, int id) => context.Chats.Where(c => c.Id == id);
    
    /// <summary>
    /// Get or create a direct message chat between the current user and a friend.
    /// Direct message chats have ProjectId = null.
    /// </summary>
    [GraphQLName("getorcreatedirectchat")]
    public async Task<Chat?> GetOrCreateDirectChat(
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
            throw new GraphQLException("Users are not friends");
        }
        
        // Find existing direct chat between these two users
        // Direct chats have ProjectId = null
        var existingChat = await context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
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
        var newChat = new Chat
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
            .FirstAsync(c => c.Id == newChat.Id);
    }
    
    /// <summary>
    /// Get all direct message chats for the current user
    /// </summary>
    [GraphQLName("mydirectchats")]
    public IQueryable<Chat> GetMyDirectChats(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return context.Chats
            .Where(c => c.ProjectId == null) // Direct message chats only
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId));
    }
}