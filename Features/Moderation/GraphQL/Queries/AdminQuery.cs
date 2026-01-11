using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Moderation.Entities;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Moderation.GraphQL.Queries;

public class AdminQuery
{
    [GraphQLName("reportedPosts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PostReport> GetReportedPosts(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if user is moderator
        var user = context.Users.Find(userId);
        if (user == null || !user.IsModerator)
        {
            throw new GraphQLException("You are not authorized to view reported posts");
        }

        // Return only unresolved reports
        return context.PostReports
            .Include(pr => pr.Post)
                .ThenInclude(p => p.User)
            .Include(pr => pr.ReportedByUser)
            .Where(pr => !pr.IsResolved);
    }
}
