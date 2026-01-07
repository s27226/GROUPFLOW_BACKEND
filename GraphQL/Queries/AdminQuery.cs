using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class AdminQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("reportedPosts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PostReport> GetReportedPosts()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if user is moderator
        var user = _context.Users.Find(userId);
        if (user == null || !user.IsModerator)
        {
            throw new GraphQLException("You are not authorized to view reported posts");
        }

        // Return only unresolved reports
        return _context.PostReports
            .Include(pr => pr.Post)
                .ThenInclude(p => p.User)
            .Include(pr => pr.ReportedByUser)
            .Where(pr => !pr.IsResolved);
    }
}
