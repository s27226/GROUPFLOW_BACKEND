namespace NAME_WIP_BACKEND.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

public class AuthMutation
{
    public async Task<AuthPayload> RegisterUser(
        [Service] AppDbContext db,
        UserRegisterInput input)
    {
        if (await db.Users.AnyAsync(u => u.Email == input.Email))
        {
            throw new GraphQLException(new Error("Email already exists", "EMAIL_EXISTS"));
        }

        var user = new User
        {
            Name = input.Name,
            Surname = input.Surname,
            Nickname = input.Nickname,
            Email = input.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(input.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwt(user);
        var refreshToken = await GenerateRefreshToken(db, user.Id);
        
        return new AuthPayload(user.Id, user.Name, user.Email, token, refreshToken, user.IsModerator);
    }

    public async Task<AuthPayload> LoginUser(
        [Service] AppDbContext db,
        UserLoginInput input)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
            {
                throw new GraphQLException(new Error("Invalid email or password", "INVALID_LOGIN"));
            }

            // Check if user is banned
            if (user.IsBanned)
            {
                if (user.BanExpiresAt == null || user.BanExpiresAt > DateTime.UtcNow)
                {
                    var banMessage = user.BanReason != null 
                        ? $"Your account has been banned. Reason: {user.BanReason}" 
                        : "Your account has been banned.";
                    
                    if (user.BanExpiresAt != null)
                    {
                        banMessage += $" Ban expires: {user.BanExpiresAt:yyyy-MM-dd HH:mm}";
                    }
                    
                    throw new GraphQLException(new Error(banMessage, "ACCOUNT_BANNED"));
                }
                else
                {
                    // Ban has expired, unban the user
                    user.IsBanned = false;
                    user.BanReason = null;
                    user.BanExpiresAt = null;
                    user.BannedByUserId = null;
                    await db.SaveChangesAsync();
                }
            }

            // Check if user is suspended
            if (user.SuspendedUntil != null && user.SuspendedUntil > DateTime.UtcNow)
            {
                throw new GraphQLException(new Error(
                    $"Your account is suspended until {user.SuspendedUntil:yyyy-MM-dd HH:mm}", 
                    "ACCOUNT_SUSPENDED"));
            }
            else if (user.SuspendedUntil != null)
            {
                // Suspension has expired, clear it
                user.SuspendedUntil = null;
                await db.SaveChangesAsync();
            }

            var token = GenerateJwt(user);
            var refreshToken = await GenerateRefreshToken(db, user.Id);
            
            return new AuthPayload(user.Id, user.Name, user.Email, token, refreshToken, user.IsModerator);
        }
        catch (GraphQLException)
        {
            throw; // Re-throw GraphQL exceptions as-is
        }
        catch (Exception)
        {
            throw new GraphQLException(new Error("An error occurred during login", "LOGIN_ERROR"));
        }
    }

    public async Task<AuthPayload> RefreshToken(
        [Service] AppDbContext db,
        string refreshToken)
    {
        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new GraphQLException(new Error("Invalid or expired refresh token", "INVALID_REFRESH_TOKEN"));
        }

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        
        // Generate new tokens
        var newAccessToken = GenerateJwt(storedToken.User);
        var newRefreshToken = await GenerateRefreshToken(db, storedToken.UserId);

        await db.SaveChangesAsync();

        return new AuthPayload(
            storedToken.User.Id, 
            storedToken.User.Name, 
            storedToken.User.Email, 
            newAccessToken, 
            newRefreshToken,
            storedToken.User.IsModerator);
    }

    public async Task<bool> RevokeRefreshToken(
        [Service] AppDbContext db,
        string refreshToken)
    {
        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        await db.SaveChangesAsync();
        return true;
    }

    private static string GenerateJwt(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("JWT_SECRET")?? throw new InvalidOperationException("JWT_SECRET not found in environment variables."))); 

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "api/v1.0.0",
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<string> GenerateRefreshToken(AppDbContext db, int userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return token;
    }
}
