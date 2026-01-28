using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
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
        Post? result = null;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (projectId.HasValue)
            {
                var project = await _context.Projects
                    .Include(p => p.Collaborators)
                    .FirstOrDefaultAsync(p => p.Id == projectId.Value);

                if (project == null)
                {
                    throw EntityNotFoundException.Project(projectId.Value);
                }

                var isOwner = project.OwnerId == userId;
                var isCollaborator = project.Collaborators.Any(c => c.UserId == userId);

                if (!isOwner && !isCollaborator)
                {
                    throw AuthorizationException.NotProjectMember();
                }
            }

            if (sharedPostId.HasValue)
            {
                var sharedPost = await _context.Posts.FindAsync(sharedPostId.Value);
                if (sharedPost == null)
                {
                    throw EntityNotFoundException.Post(sharedPostId.Value);
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
            result = post;
        });

        return result!;
    }

    public async Task<PostLike> LikePostAsync(int userId, int postId)
    {
        PostLike? result = null;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);
            
            if (post == null)
            {
                throw EntityNotFoundException.Post(postId);
            }

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

            if (existingLike != null)
            {
                throw DuplicateEntityException.PostLike();
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
            result = postLike;
        });

        return result!;
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
        PostComment? result = null;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                throw EntityNotFoundException.Post(postId);
            }

            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.PostComments.FindAsync(parentCommentId.Value);
                if (parentComment == null)
                {
                    throw EntityNotFoundException.Comment(parentCommentId.Value);
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
            result = comment;
        });

        return result!;
    }

    public async Task<bool> DeleteCommentAsync(int userId, int commentId)
    {
        var result = false;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var comment = await _context.PostComments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                result = false;
                return;
            }

            if (comment.UserId != userId)
            {
                throw AuthorizationException.NotCommentOwner();
            }

            if (comment.Replies != null && comment.Replies.Any())
            {
                _context.PostComments.RemoveRange(comment.Replies);
            }

            _context.PostComments.Remove(comment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            result = true;
        });

        return result;
    }

    public async Task<PostLike> LikeCommentAsync(int userId, int commentId)
    {
        PostLike? result = null;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var comment = await _context.PostComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            
            if (comment == null)
            {
                throw EntityNotFoundException.Comment(commentId);
            }

            var existingLike = await _context.PostCommentLikes
                .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.PostCommentId == commentId);

            if (existingLike != null)
            {
                throw DuplicateEntityException.CommentLike();
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

            result = new PostLike { UserId = userId, PostId = comment.PostId, CreatedAt = commentLike.CreatedAt };
        });

        return result!;
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
            throw EntityNotFoundException.Post(postId);
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
        var result = false;
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var post = await _context.Posts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                result = false;
                return;
            }

            if (post.UserId != userId)
            {
                throw AuthorizationException.NotPostOwner();
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
            result = true;
        });

        return result;
    }
}
