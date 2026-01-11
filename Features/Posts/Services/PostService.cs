using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Notifications.Entities;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.Services;

/// <summary>
/// Service implementation for post operations
/// Extracts business logic from GraphQL mutations/queries
/// Uses transactions for consistency
/// </summary>
public class PostService : IPostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Post> CreatePostAsync(int userId, int? projectId, string title, string description, string content, string? imageUrl, int? sharedPostId, bool isPublic)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // If projectId is provided, verify the user is a member
            if (projectId.HasValue)
            {
                var project = await _context.Projects
                    .Include(p => p.Collaborators)
                    .FirstOrDefaultAsync(p => p.Id == projectId.Value);

                if (project == null)
                {
                    throw new InvalidOperationException("Project not found");
                }

                var isOwner = project.OwnerId == userId;
                var isCollaborator = project.Collaborators.Any(c => c.UserId == userId);

                if (!isOwner && !isCollaborator)
                {
                    throw new UnauthorizedAccessException("You are not a member of this project");
                }
            }

            // If sharedPostId is provided, verify the post exists
            if (sharedPostId.HasValue)
            {
                var sharedPost = await _context.Posts.FindAsync(sharedPostId.Value);
                if (sharedPost == null)
                {
                    throw new InvalidOperationException("Shared post not found");
                }
            }

            var post = new Post
            {
                UserId = userId,
                ProjectId = projectId,
                Title = title,
                Description = description,
                Content = content,
                ImageUrl = imageUrl,
                SharedPostId = sharedPostId,
                Public = isPublic,
                Created = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            
            await _context.SaveChangesAsync();
            await _context.Entry(post).Reference(p => p.User).LoadAsync();

            await transaction.CommitAsync();
            return post;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PostLike> LikePostAsync(int userId, int postId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);
            
            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

            if (existingLike != null)
            {
                throw new InvalidOperationException("Post is already liked");
            }

            var postLike = new PostLike
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(postLike);

            if (post.UserId != userId)
            {
                var liker = await _context.Users.FindAsync(userId);
                var notification = new Notification
                {
                    UserId = post.UserId,
                    Type = "POST_LIKE",
                    Message = $"{liker?.Nickname ?? liker?.Name} liked your post",
                    ActorUserId = userId,
                    PostId = postId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return postLike;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UnlikePostAsync(int userId, int postId)
    {
        var postLike = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

        if (postLike == null)
        {
            return false;
        }

        _context.PostLikes.Remove(postLike);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PostComment> AddCommentAsync(int userId, int postId, string content, int? parentCommentId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.PostComments.FindAsync(parentCommentId.Value);
                if (parentComment == null)
                {
                    throw new InvalidOperationException("Parent comment not found");
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

            _context.PostComments.Add(comment);

            if (post.UserId != userId)
            {
                var commenter = await _context.Users.FindAsync(userId);
                var notification = new Notification
                {
                    UserId = post.UserId,
                    Type = "POST_COMMENT",
                    Message = $"{commenter?.Nickname ?? commenter?.Name} commented on your post",
                    ActorUserId = userId,
                    PostId = postId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            await transaction.CommitAsync();
            return comment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteCommentAsync(int userId, int commentId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var comment = await _context.PostComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return false;
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            if (comment.Replies != null && comment.Replies.Any())
            {
                _context.PostComments.RemoveRange(comment.Replies);
            }

            _context.PostComments.Remove(comment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PostLike> LikeCommentAsync(int userId, int commentId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var comment = await _context.PostComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            
            if (comment == null)
            {
                throw new InvalidOperationException("Comment not found");
            }

            var existingLike = await _context.PostCommentLikes
                .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.PostCommentId == commentId);

            if (existingLike != null)
            {
                throw new InvalidOperationException("Comment is already liked");
            }

            var commentLike = new PostCommentLike
            {
                UserId = userId,
                PostCommentId = commentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostCommentLikes.Add(commentLike);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PostLike { UserId = userId, PostId = comment.PostId, CreatedAt = commentLike.CreatedAt };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UnlikeCommentAsync(int userId, int commentId)
    {
        var commentLike = await _context.PostCommentLikes
            .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.PostCommentId == commentId);

        if (commentLike == null)
        {
            return false;
        }

        _context.PostCommentLikes.Remove(commentLike);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SavePostAsync(int userId, int postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new InvalidOperationException("Post not found");
        }

        var existingSave = await _context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (existingSave != null)
        {
            return false;
        }

        var savedPost = new SavedPost
        {
            UserId = userId,
            PostId = postId,
            SavedAt = DateTime.UtcNow
        };

        _context.SavedPosts.Add(savedPost);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnsavePostAsync(int userId, int postId)
    {
        var savedPost = await _context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (savedPost == null)
        {
            return false;
        }

        _context.SavedPosts.Remove(savedPost);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePostAsync(int userId, int postId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var post = await _context.Posts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return false;
            }

            if (post.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own posts");
            }

            if (post.Likes != null && post.Likes.Any())
            {
                _context.PostLikes.RemoveRange(post.Likes);
            }

            if (post.Comments != null && post.Comments.Any())
            {
                _context.PostComments.RemoveRange(post.Comments);
            }

            var savedPosts = await _context.SavedPosts
                .Where(sp => sp.PostId == postId)
                .ToListAsync();
            if (savedPosts.Any())
            {
                _context.SavedPosts.RemoveRange(savedPosts);
            }

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
