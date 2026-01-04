using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Types;

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
