using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Moderation.Inputs;

public record BanUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    [property: StringLength(300, MinimumLength = 5)]
    string Reason,
    
    DateTime? ExpiresAt
);

public record SuspendUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    DateTime SuspendedUntil
);

public record ResetPasswordInput(
    [property: Range(1, int.MaxValue)]
    int UserId,

    [property: Required]
    [property: MinLength(8)]
    string NewPassword
);

public record ManageUserRoleInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    bool IsModerator
);
