using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class PostMutation
{
    [GraphQLName("likePost")]
    public async Task<PostLike> LikePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // Check if already liked
        var existingLike = await context.PostLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

        if (existingLike != null)
        {
            throw new GraphQLException("Post is already liked");
        }

        var postLike = new PostLike
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostLikes.Add(postLike);
        await context.SaveChangesAsync();

        return postLike;
    }

    [GraphQLName("unlikePost")]
    public async Task<bool> UnlikePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var postLike = await context.PostLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

        if (postLike == null)
        {
            throw new GraphQLException("Post is not liked");
        }

        context.PostLikes.Remove(postLike);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("addComment")]
    public async Task<PostComment> AddComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string content,
        int? parentCommentId = null)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // If replying to a comment, check if parent comment exists
        if (parentCommentId.HasValue)
        {
            var parentComment = await context.PostComments.FindAsync(parentCommentId.Value);
            if (parentComment == null)
            {
                throw new GraphQLException("Parent comment not found");
            }
        }

        var comment = new PostComment
        {
            UserId = userId,
            PostId = postId,
            Content = content,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostComments.Add(comment);
        await context.SaveChangesAsync();

        // Reload the comment with User navigation property
        await context.Entry(comment)
            .Reference(c => c.User)
            .LoadAsync();

        return comment;
    }

    [GraphQLName("deleteComment")]
    public async Task<bool> DeleteComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var comment = await context.PostComments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new GraphQLException("Comment not found");
        }

        // Only the comment author can delete it
        if (comment.UserId != userId)
        {
            throw new GraphQLException("You are not authorized to delete this comment");
        }

        // Note: Cascade delete will remove all replies and likes
        context.PostComments.Remove(comment);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("likeComment")]
    public async Task<PostCommentLike> LikeComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if comment exists
        var comment = await context.PostComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new GraphQLException("Comment not found");
        }

        // Check if already liked
        var existingLike = await context.PostCommentLikes
            .FirstOrDefaultAsync(pcl => pcl.UserId == userId && pcl.PostCommentId == commentId);

        if (existingLike != null)
        {
            throw new GraphQLException("Comment is already liked");
        }

        var commentLike = new PostCommentLike
        {
            UserId = userId,
            PostCommentId = commentId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostCommentLikes.Add(commentLike);
        await context.SaveChangesAsync();

        return commentLike;
    }

    [GraphQLName("unlikeComment")]
    public async Task<bool> UnlikeComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var commentLike = await context.PostCommentLikes
            .FirstOrDefaultAsync(pcl => pcl.UserId == userId && pcl.PostCommentId == commentId);

        if (commentLike == null)
        {
            throw new GraphQLException("Comment is not liked");
        }

        context.PostCommentLikes.Remove(commentLike);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("sharePost")]
    public async Task<Post> SharePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string? content = null,
        int? projectId = null)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if original post exists
        var originalPost = await context.Posts.FindAsync(postId);
        if (originalPost == null)
        {
            throw new GraphQLException("Post not found");
        }

        var sharedPost = new Post
        {
            UserId = userId,
            ProjectId = projectId,
            Title = "Shared Post",
            Description = content ?? "",
            Content = content ?? $"Shared a post",
            SharedPostId = postId,
            Public = true,
            Created = DateTime.UtcNow
        };

        context.Posts.Add(sharedPost);
        await context.SaveChangesAsync();

        return sharedPost;
    }
}
