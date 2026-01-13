using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Auth.GraphQL.Inputs;

public record UserRegisterInput(
    [property: Required]
    [property: StringLength(50)]
    string Name,

    [property: Required]
    [property: StringLength(50)]
    string Surname,

    [property: Required]
    [property: StringLength(30)]
    string Nickname,

    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    [property: MinLength(8)]
    string Password
);

public record UserLoginInput(
    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    string Password
);

public record ResetPasswordInput(
    [property: Required]
    [property: EmailAddress]
    string Email,
    
    [property: Required]
    string Token,
    
    [property: Required]
    [property: MinLength(8)]
    string NewPassword
);

public record ChangePasswordInput(
    [property: Required]
    string CurrentPassword,
    
    [property: Required]
    [property: MinLength(8)]
    string NewPassword
);
