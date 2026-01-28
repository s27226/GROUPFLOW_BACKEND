namespace GROUPFLOW.Common.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions.
/// Error codes should be i18n keys (e.g., "errors.USER_NOT_FOUND") for frontend translation.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    
    protected DomainException(string errorCode) : base(errorCode) 
    { 
        ErrorCode = errorCode;
    }
    protected DomainException(string errorCode, Exception innerException) : base(errorCode, innerException) 
    { 
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base($"errors.{entityType.ToUpperInvariant()}_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public static EntityNotFoundException User(int id) => new("User", id);
    public static EntityNotFoundException Project(int id) => new("Project", id);
    public static EntityNotFoundException Post(int id) => new("Post", id);
    public static EntityNotFoundException Comment(int id) => new("Comment", id);
    public static EntityNotFoundException Chat(int id) => new("Chat", id);
    public static EntityNotFoundException Notification(int id) => new("Notification", id);
    public static EntityNotFoundException Friendship() => new("Friendship");
    public static EntityNotFoundException FriendRequest(int id) => new("FriendRequest", id);
    public static EntityNotFoundException ProjectInvitation(int id) => new("ProjectInvitation", id);
    public static EntityNotFoundException ProjectEvent(int id) => new("ProjectEvent", id);
    public static EntityNotFoundException UserSkill(int id) => new("UserSkill", id);
    public static EntityNotFoundException UserInterest(int id) => new("UserInterest", id);
    public static EntityNotFoundException SavedPost() => new("SavedPost");
    public static EntityNotFoundException BlockedUser() => new("BlockedUser");
    public static EntityNotFoundException PostReport(int id) => new("PostReport", id);
    public static EntityNotFoundException BlobFile(int id) => new("BlobFile", id);
}

/// <summary>
/// Thrown when a user is not authenticated.
/// </summary>
public class AuthenticationException : DomainException
{
    public AuthenticationException(string errorCode = "errors.NOT_AUTHENTICATED") : base(errorCode) { }
}

/// <summary>
/// Thrown when a user is not authorized to perform an action.
/// </summary>
public class AuthorizationException : DomainException
{
    public AuthorizationException(string errorCode = "errors.NOT_AUTHORIZED") : base(errorCode) { }
    
    public static AuthorizationException NotProjectMember() => new("errors.NOT_PROJECT_MEMBER");
    public static AuthorizationException NotProjectOwner() => new("errors.NOT_PROJECT_OWNER");
    public static AuthorizationException NotPostOwner() => new("errors.NOT_POST_OWNER");
    public static AuthorizationException NotCommentOwner() => new("errors.NOT_COMMENT_OWNER");
    public static AuthorizationException NotInvitationRecipient() => new("errors.NOT_INVITATION_RECIPIENT");
    public static AuthorizationException NotFriendRequestRecipient() => new("errors.NOT_FRIEND_REQUEST_RECIPIENT");
    public static AuthorizationException CannotDeleteEvent() => new("errors.CANNOT_DELETE_EVENT");
    public static AuthorizationException NotModerator() => new("errors.NOT_MODERATOR");
    public static AuthorizationException CannotDeleteReportedPost() => new("errors.CANNOT_DELETE_REPORTED_POST");
    public static AuthorizationException CannotDiscardReport() => new("errors.CANNOT_DISCARD_REPORT");
    public static AuthorizationException CannotDeleteFile() => new("errors.CANNOT_DELETE_FILE");
    public static AuthorizationException CannotUpdateProfilePicture() => new("errors.CANNOT_UPDATE_PROFILE_PICTURE");
    public static AuthorizationException CannotUpdateBanner() => new("errors.CANNOT_UPDATE_BANNER");
    public static AuthorizationException CannotUpdateProjectImage() => new("errors.CANNOT_UPDATE_PROJECT_IMAGE");
    public static AuthorizationException CannotUpdateProjectBanner() => new("errors.CANNOT_UPDATE_PROJECT_BANNER");
    public static AuthorizationException CannotUploadProjectMedia() => new("errors.CANNOT_UPLOAD_PROJECT_MEDIA");
    public static AuthorizationException CannotUploadProjectFiles() => new("errors.CANNOT_UPLOAD_PROJECT_FILES");
    public static AuthorizationException CannotUploadPostImages() => new("errors.CANNOT_UPLOAD_POST_IMAGES");
}

/// <summary>
/// Thrown when trying to create a duplicate entity.
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string? Field { get; }

    public DuplicateEntityException(string entityType, string? field = null)
        : base(field != null 
            ? $"errors.{entityType.ToUpperInvariant()}_{field.ToUpperInvariant()}_EXISTS" 
            : $"errors.{entityType.ToUpperInvariant()}_EXISTS")
    {
        EntityType = entityType;
        Field = field;
    }

    public static DuplicateEntityException Email() => new("User", "email");
    public static DuplicateEntityException Nickname() => new("User", "nickname");
    public static DuplicateEntityException Friendship() => new("Friendship", "users");
    public static DuplicateEntityException PostLike() => new("PostLike");
    public static DuplicateEntityException CommentLike() => new("CommentLike");
    public static DuplicateEntityException AlreadySaved() => new("SavedPost");
    public static DuplicateEntityException ProjectInvitation() => new("ProjectInvitation");
    public static DuplicateEntityException PostReport() => new("PostReport");
    public static DuplicateEntityException BlockedUser() => new("BlockedUser");
}

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string errorCode) : base(errorCode) { }

    public static BusinessRuleException CannotFriendSelf() => new("errors.CANNOT_FRIEND_SELF");
    public static BusinessRuleException CannotFriendYourself() => new("errors.CANNOT_FRIEND_SELF");
    public static BusinessRuleException AlreadyFriends() => new("errors.ALREADY_FRIENDS");
    public static BusinessRuleException FriendRequestPending() => new("errors.FRIEND_REQUEST_PENDING");
    public static BusinessRuleException CannotBlockYourself() => new("errors.CANNOT_BLOCK_SELF");
    public static BusinessRuleException CannotBlockFriend() => new("errors.CANNOT_BLOCK_FRIEND");
    public static BusinessRuleException CannotLikeOwnPost() => new("errors.CANNOT_LIKE_OWN_POST");
    public static BusinessRuleException UserAlreadyProjectMember() => new("errors.USER_ALREADY_PROJECT_MEMBER");
    public static BusinessRuleException CanOnlyInviteFriends() => new("errors.CAN_ONLY_INVITE_FRIENDS");
    public static BusinessRuleException CannotRemoveProjectOwner() => new("errors.CANNOT_REMOVE_PROJECT_OWNER");
    public static BusinessRuleException UserNotProjectMember() => new("errors.USER_NOT_PROJECT_MEMBER");
    public static BusinessRuleException InvitationExpired() => new("errors.INVITATION_EXPIRED");
    public static BusinessRuleException UsersNotFriends() => new("errors.USERS_NOT_FRIENDS");
}

/// <summary>
/// Thrown when input validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string errorCode = "errors.VALIDATION_FAILED", IDictionary<string, string[]>? errors = null)
        : base(errorCode)
    {
        Errors = errors != null 
            ? new Dictionary<string, string[]>(errors) 
            : new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string errorCode)
        : base("errors.VALIDATION_FAILED")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { errorCode } } };
    }

    // Blob/File validation errors
    public static ValidationException InvalidBlobType(string blobType) => new("errors.INVALID_BLOB_TYPE");
    public static ValidationException InvalidBase64Data() => new("errors.INVALID_BASE64_DATA");
    public static ValidationException FileSizeExceeded() => new("errors.FILE_SIZE_EXCEEDED");
    public static ValidationException FileTypeNotAllowed() => new("errors.FILE_TYPE_NOT_ALLOWED");
}

/// <summary>
/// Thrown when authentication-related errors occur (login, token, password).
/// </summary>
public class AuthErrorException : DomainException
{
    public AuthErrorException(string errorCode) : base(errorCode) { }

    public static AuthErrorException InvalidLogin() => new("errors.INVALID_LOGIN");
    public static AuthErrorException AccountBanned(string? reason = null, DateTime? expiresAt = null) => new("errors.ACCOUNT_BANNED");
    public static AuthErrorException AccountSuspended(DateTime suspendedUntil) => new("errors.ACCOUNT_SUSPENDED");
    public static AuthErrorException LoginError() => new("errors.LOGIN_ERROR");
    public static AuthErrorException NoRefreshToken() => new("errors.NO_REFRESH_TOKEN");
    public static AuthErrorException InvalidTokenType() => new("errors.INVALID_TOKEN_TYPE");
    public static AuthErrorException InvalidToken() => new("errors.INVALID_TOKEN");
    public static AuthErrorException UserBanned() => new("errors.USER_BANNED");
    public static AuthErrorException TokenExpired() => new("errors.TOKEN_EXPIRED");
    public static AuthErrorException InvalidPassword() => new("errors.INVALID_PASSWORD");
}
