using System.Security.Claims;
using HotChocolate.Authorization;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

public class UsersQuery
{
    [GraphQLName("allusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context) => context.Users;
    
    [GraphQLName("getuserbyid")]
    [UseProjection]
    public User? GetUserById(AppDbContext context, int id) => context.Users.FirstOrDefault(g => g.Id == id);
    
    [Authorize]
    [GraphQLName("me")]
    [UseProjection]
    public User? GetCurrentUser(AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        
        return context.Users.FirstOrDefault(u => u.Id == userId);
    }
}