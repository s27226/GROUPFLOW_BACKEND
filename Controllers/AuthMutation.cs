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
using NAME_WIP_BACKEND.GraphQL.Responses;

public class AuthMutation
{
    private readonly AppDbContext _db;

    public AuthMutation(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthPayloadResponse> RegisterUser(
        UserRegisterInput input,
        CancellationToken ct = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            if (await _db.Users.AnyAsync(u => u.Email == input.Email, ct))
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

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            var token = GenerateJwt(user);
            var refreshToken = await GenerateRefreshToken(_db, user.Id, ct);
            
            await transaction.CommitAsync(ct);
            return new AuthPayloadResponse(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, token, refreshToken, user.IsModerator);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AuthPayloadResponse> LoginUser(
        UserLoginInput input,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == input.Email, ct);
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
            {
                throw new GraphQLException(new Error("Invalid email or password", "INVALID_LOGIN"));
            }

            await HandleUserBanAsync(_db, user, ct);
            await HandleUserSuspensionAsync(_db, user, ct);

            var token = GenerateJwt(user);
            var refreshToken = await GenerateRefreshToken(_db, user.Id, ct);
            
            return new AuthPayloadResponse(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, token, refreshToken, user.IsModerator);
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

    public async Task<AuthPayloadResponse> RefreshToken(
        string refreshToken,
        CancellationToken ct = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var storedToken = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new GraphQLException(new Error("Invalid or expired refresh token", "INVALID_REFRESH_TOKEN"));
            }

            // Revoke the old refresh token
            storedToken.IsRevoked = true;
            
            // Generate new tokens
            var newAccessToken = GenerateJwt(storedToken.User);
            var newRefreshToken = await GenerateRefreshToken(_db, storedToken.UserId, ct);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return new AuthPayloadResponse(
                storedToken.User.Id, 
                storedToken.User.Name, 
                storedToken.User.Surname, 
                storedToken.User.Nickname, 
                storedToken.User.Email, 
                storedToken.User.ProfilePic, 
                newAccessToken, 
                newRefreshToken,
                storedToken.User.IsModerator);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> RevokeRefreshToken(
        string refreshToken,
        CancellationToken ct = default)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
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

    private static async Task<string> GenerateRefreshToken(AppDbContext _db, int userId, CancellationToken ct = default)
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

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return token;
    }

    private static async Task HandleUserBanAsync(AppDbContext _db, User user, CancellationToken ct)
    {
        if (!user.IsBanned) return;

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
            await _db.SaveChangesAsync(ct);
        }
    }

    private static async Task HandleUserSuspensionAsync(AppDbContext _db, User user, CancellationToken ct)
    {
        if (user.SuspendedUntil == null) return;

        if (user.SuspendedUntil > DateTime.UtcNow)
        {
            throw new GraphQLException(new Error(
                $"Your account is suspended until {user.SuspendedUntil:yyyy-MM-dd HH:mm}", 
                "ACCOUNT_SUSPENDED"));
        }
        else
        {
            // Suspension has expired, clear it
            user.SuspendedUntil = null;
            await _db.SaveChangesAsync(ct);
        }
    }
}
