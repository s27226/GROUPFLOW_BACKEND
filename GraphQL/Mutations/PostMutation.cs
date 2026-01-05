using System.Security.Claims;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class PostMutation
{
    private readonly PostService _service;

    public PostMutation(PostService service)
    {
        _service = service;
    }

    [GraphQLName("createPost")]
    public Task<Post> CreatePost(
        PostInput input,
        ClaimsPrincipal user)
        => _service.CreatePost(user, input);

    [GraphQLName("likePost")]
    public Task<PostLike> LikePost(int postId, ClaimsPrincipal user)
        => _service.LikePost(user, postId);

    [GraphQLName("unlikePost")]
    public Task<bool> UnlikePost(int postId, ClaimsPrincipal user)
        => _service.UnlikePost(user, postId);

    [GraphQLName("addComment")]
    public Task<PostComment> AddComment(
        int postId,
        string content,
        int? parentCommentId,
        ClaimsPrincipal user)
        => _service.AddComment(user, postId, content, parentCommentId);

    [GraphQLName("deleteComment")]
    public Task<bool> DeleteComment(int commentId, ClaimsPrincipal user)
        => _service.DeleteComment(user, commentId);

    [GraphQLName("likeComment")]
    public Task<PostCommentLike> LikeComment(int commentId, ClaimsPrincipal user)
        => _service.LikeComment(user, commentId);

    [GraphQLName("unlikeComment")]
    public Task<bool> UnlikeComment(int commentId, ClaimsPrincipal user)
        => _service.UnlikeComment(user, commentId);
    
    [GraphQLName("sharePost")]
    public Task<Post> SharePost(
        int postId,
        string? content,
        int? projectId,
        ClaimsPrincipal user)
        => _service.SharePost(user, postId, content, projectId);

    [GraphQLName("reportPost")]
    public Task<PostReport> ReportPost(
        ReportPostInput input,
        ClaimsPrincipal user)
        => _service.ReportPost(user, input);

    [GraphQLName("deleteReportedPost")]
    public Task<bool> DeleteReportedPost(
        int postId,
        ClaimsPrincipal user)
        => _service.DeleteReportedPost(user, postId);

    [GraphQLName("discardReport")]
    public Task<bool> DiscardReport(
        int reportId,
        ClaimsPrincipal user)
        => _service.DiscardReport(user, reportId);
}



