using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Features.Users.Entities;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GROUPFLOW.Features.Users.GraphQL.Mutations;

public class UserMutation
{
    [Authorize]
    public async Task<User> UpdateUserProfile(
        UpdateUserProfileInput input,
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        {
            throw new AuthenticationException();
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        
        if (user == null)
        {
            throw EntityNotFoundException.User(currentUserId);
        }

        // Update fields if provided
        if (input.Name != null)
        {
            user.Name = input.Name;
        }

        if (input.Surname != null)
        {
            user.Surname = input.Surname;
        }

        if (input.Nickname != null)
        {
            // Check if nickname is already taken by another user
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Nickname == input.Nickname && u.Id != currentUserId);
            
            if (existingUser != null)
            {
                throw DuplicateEntityException.Nickname();
            }

            user.Nickname = input.Nickname;
        }

        if (input.Bio != null)
        {
            user.Bio = input.Bio;
        }

        if (input.DateOfBirth.HasValue)
        {
            user.DateOfBirth = input.DateOfBirth.Value;
        }

        await context.SaveChangesAsync();

        return user;
    }
}

public record UpdateUserProfileInput(
    string? Name,
    string? Surname,
    string? Nickname,
    string? Bio,
    DateTime? DateOfBirth
);
