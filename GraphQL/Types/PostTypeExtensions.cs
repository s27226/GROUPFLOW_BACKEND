using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Types;

[ExtendObjectType(typeof(Post))]
public class PostTypeExtensions
{
    /// <summary>
    /// Returns only top-level comments (not replies) for a post
    /// </summary>
    [UseProjection]
    public IQueryable<PostComment> GetComments(
        [Parent] Post post,
        [Service] AppDbContext context)
    {
        return context.PostComments
            .Where(c => c.PostId == post.Id && c.ParentCommentId == null);
    }
}
