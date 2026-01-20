using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.GraphQL.Extensions;

[ExtendObjectType(typeof(Post))]
public class PostTypeExtensions
{
    /// <summary>
    /// Returns only top-level comments (not replies) for a post
    /// </summary>
    public async Task<List<PostComment>> GetComments(
        [Parent] Post post,
        [Service] AppDbContext context)
    {
        return await context.PostComments
            .Include(c => c.User)
                .ThenInclude(u => u.ProfilePicBlob)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Where(c => c.PostId == post.Id && c.ParentCommentId == null)
            .ToListAsync();
    }
}
