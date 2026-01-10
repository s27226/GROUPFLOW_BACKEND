namespace NAME_WIP_BACKEND.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        [Service] IHttpContextAccessor httpContextAccessor,
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

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);
        SetAuthCookies(httpContextAccessor.HttpContext!, accessToken, refreshToken);
        
        return new AuthPayload(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, user.IsModerator);
    }

    public async Task<AuthPayload> LoginUser(
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor,
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

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user);
            SetAuthCookies(httpContextAccessor.HttpContext!, accessToken, refreshToken);
            
            return new AuthPayload(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, user.IsModerator);
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

    public bool Logout([Service] IHttpContextAccessor httpContextAccessor)
    {
        ClearAuthCookies(httpContextAccessor.HttpContext!);
        return true;
    }

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<AuthPayload> RefreshToken(
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext!;
        
        // Get refresh token from cookie
        if (!context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            throw new GraphQLException(new Error("No refresh token provided", "NO_REFRESH_TOKEN"));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? throw new InvalidOperationException("JWT_SECRET not found"));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
            
            // Verify this is actually a refresh token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "refresh")
            {
                throw new GraphQLException(new Error("Invalid token type", "INVALID_TOKEN_TYPE"));
            }
            
            // Get user ID from token claims - check both "sub" and ClaimTypes.NameIdentifier
            // because ASP.NET may map the claim differently depending on context
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new GraphQLException(new Error("Invalid token claims", "INVALID_TOKEN"));
            }

            // Get user from database
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException(new Error("User not found", "USER_NOT_FOUND"));
            }

            // Check if user is banned or suspended
            if (user.IsBanned)
            {
                throw new GraphQLException(new Error("User is banned", "USER_BANNED"));
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);
            SetAuthCookies(context, newAccessToken, newRefreshToken);

            return new AuthPayload(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, user.IsModerator);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new GraphQLException(new Error("Refresh token expired", "TOKEN_EXPIRED"));
        }
        catch (Exception ex) when (ex is not GraphQLException)
        {
            throw new GraphQLException(new Error("Invalid refresh token", "INVALID_TOKEN"));
        }
    }

    private static string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("token_type", "access")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET not found")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "api/v1.0.0",
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Short-lived access token
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("token_type", "refresh")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET not found")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "api/v1.0.0",
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Long-lived refresh token
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void SetAuthCookies(HttpContext context, string accessToken, string refreshToken)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None, // Changed to None for cross-origin requests
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None, // Changed to None for cross-origin requests
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        context.Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        context.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    private static void ClearAuthCookies(HttpContext context)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None // Changed to None for cross-origin requests
        };

        context.Response.Cookies.Delete("access_token", cookieOptions);
        context.Response.Cookies.Delete("refresh_token", cookieOptions);
    }
}
