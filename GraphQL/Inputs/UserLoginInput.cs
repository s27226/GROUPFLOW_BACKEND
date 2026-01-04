using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.Models;

public record UserLoginInput(
    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    string Password
);