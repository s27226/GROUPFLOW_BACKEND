using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Chat.Entities;

namespace GROUPFLOW.Features.Chat.GraphQL.Queries;

public class UserChatQuery
{
    /// <summary>
    /// Get all UserChats for the current user (for chat list)
    /// </summary>
    [GraphQLName("myuserchats")]
    public async Task<List<UserChat>> GetMyUserChats(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return await context.UserChats
            .Include(uc => uc.Chat)
                .ThenInclude(c => c.UserChats)
                    .ThenInclude(uc => uc.User)
                        .ThenInclude(u => u.ProfilePicBlob)
            .Where(uc => uc.UserId == userId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get the UserChat ID for the current user in a specific chat
    /// </summary>
    [GraphQLName("myuserchat")]
    public UserChat? GetMyUserChat(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int chatId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return context.UserChats
            .FirstOrDefault(uc => uc.ChatId == chatId && uc.UserId == userId);
    }
}
