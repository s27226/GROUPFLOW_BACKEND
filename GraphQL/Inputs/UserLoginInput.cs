using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.Models;

public record UserLoginInput(
    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    string Password
);