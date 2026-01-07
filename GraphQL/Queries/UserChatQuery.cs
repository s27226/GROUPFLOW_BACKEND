using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class UserChatQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserChatQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Get the UserChat ID for the current user in a specific chat
    /// </summary>
    [GraphQLName("myuserchat")]
    public UserChat? GetMyUserChat(int chatId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return _context.UserChats
            .FirstOrDefault(uc => uc.ChatId == chatId && uc.UserId == userId);
    }
}