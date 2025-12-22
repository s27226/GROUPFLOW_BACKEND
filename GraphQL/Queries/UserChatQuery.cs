using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class UserChatQuery
{
    
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