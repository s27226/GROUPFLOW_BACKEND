using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ChatQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("allchats")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Chat> GetChats() => _context.Chats;
    
    [GraphQLName("chatbyid")]
    [UseProjection]
    public IQueryable<Chat> GetChatById(int id) => _context.Chats.Where(c => c.Id == id);
    
    /// <summary>
    /// Get or create a direct message chat between the current user and a friend.
    /// Direct message chats have ProjectId = null.
    /// </summary>
    [GraphQLName("getorcreatedirectchat")]
    public async Task<Chat?> GetOrCreateDirectChat(int friendId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if users are friends
        var areFriends = await _context.Friendships
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
        var existingChat = await _context.Chats
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
        
        _context.Chats.Add(newChat);
        await _context.SaveChangesAsync();
        
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
        
        _context.UserChats.Add(userChat1);
        _context.UserChats.Add(userChat2);
        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await _context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
            .FirstAsync(c => c.Id == newChat.Id);
    }
    
    /// <summary>
    /// Get all direct message chats for the current user
    /// </summary>
    [GraphQLName("mydirectchats")]
    public IQueryable<Chat> GetMyDirectChats()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return _context.Chats
            .Where(c => c.ProjectId == null) // Direct message chats only
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId));
    }
}