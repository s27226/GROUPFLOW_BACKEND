using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostResponse(
    int Id,
    string Title,
    string Description,
    string Content,
    string? ImageUrl,
    bool Public,
    DateTime Created,
    UserResponse User,
    int LikesCount,
    int CommentsCount
)
{
    public static PostResponse FromPost(Post post)
    {
        return new PostResponse(
            post.Id,
            post.Title,
            post.Description,
            post.Content,
            post.ImageUrl,
            post.Public,
            post.Created,
            UserResponse.FromUser(post.User),
            post.Likes.Count,
            post.Comments.Count
        );
    }
}