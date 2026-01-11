using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.Services;

/// <summary>
/// Service interface for post operations - isolates business logic
/// </summary>
public interface IPostService
{
    Task<Post> CreatePostAsync(int userId, int? projectId, string title, string description, string content, string? imageUrl, int? sharedPostId, bool isPublic);
    Task<PostLike> LikePostAsync(int userId, int postId);
    Task<bool> UnlikePostAsync(int userId, int postId);
    Task<PostComment> AddCommentAsync(int userId, int postId, string content, int? parentCommentId);
    Task<bool> DeleteCommentAsync(int userId, int commentId);
    Task<PostLike> LikeCommentAsync(int userId, int commentId);
    Task<bool> UnlikeCommentAsync(int userId, int commentId);
    Task<bool> SavePostAsync(int userId, int postId);
    Task<bool> UnsavePostAsync(int userId, int postId);
    Task<bool> DeletePostAsync(int userId, int postId);
}
