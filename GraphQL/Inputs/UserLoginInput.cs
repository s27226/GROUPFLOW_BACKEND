using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.Models;

public record UserLoginInput(
    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    string Password
);