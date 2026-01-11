namespace GROUPFLOW.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base($"{entityType} not found" + (entityId != null ? $" (ID: {entityId})" : ""))
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
}

/// <summary>
/// Thrown when a user is not authenticated.
/// </summary>
public class AuthenticationException : DomainException
{
    public AuthenticationException(string message = "User not authenticated") : base(message) { }
}

/// <summary>
/// Thrown when a user is not authorized to perform an action.
/// </summary>
public class AuthorizationException : DomainException
{
    public AuthorizationException(string message = "You are not authorized to perform this action") : base(message) { }
    
    public static AuthorizationException NotProjectMember() => new("You are not a member of this project");
    public static AuthorizationException NotProjectOwner() => new("You must be the project owner to perform this action");
    public static AuthorizationException NotPostOwner() => new("You must be the post owner to perform this action");
}

/// <summary>
/// Thrown when a duplicate entity or action is detected.
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string? Field { get; }

    public DuplicateEntityException(string entityType, string? field = null)
        : base(field != null ? $"{entityType} with this {field} already exists" : $"{entityType} already exists")
    {
        EntityType = entityType;
        Field = field;
    }

    public static DuplicateEntityException AlreadyLiked() => new("Like");
    public static DuplicateEntityException AlreadySaved() => new("SavedPost");
    public static DuplicateEntityException UserExists(string field) => new("User", field);
}

/// <summary>
/// Thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error) : base(error)
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { error }
        };
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
    
    public static BusinessRuleException CannotBlockYourself() => new("You cannot block yourself");
    public static BusinessRuleException CannotFriendYourself() => new("You cannot send a friend request to yourself");
    public static BusinessRuleException AlreadyFriends() => new("You are already friends with this user");
    public static BusinessRuleException FriendRequestPending() => new("A friend request is already pending");
}
