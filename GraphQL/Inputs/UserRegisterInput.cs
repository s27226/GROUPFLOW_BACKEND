using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.Models;

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
