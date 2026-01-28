using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Auth.Entities;
using GROUPFLOW.Features.Auth.GraphQL.Inputs;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Auth.GraphQL.Mutations;

public class AuthMutation
{
    public async Task<AuthPayload> RegisterUser(
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor,
        UserRegisterInput input)
    {
        input.ValidateInput();
        
        if (await db.Users.AnyAsync(u => u.Email == input.Email))
        {
            throw DuplicateEntityException.Email();
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
        input.ValidateInput();
        
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
            {
                throw AuthErrorException.InvalidLogin();
            }

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
                    
                    throw AuthErrorException.AccountBanned(user.BanReason, user.BanExpiresAt);
                }
                else
                {
                    user.IsBanned = false;
                    user.BanReason = null;
                    user.BanExpiresAt = null;
                    user.BannedByUserId = null;
                    await db.SaveChangesAsync();
                }
            }

            if (user.SuspendedUntil != null && user.SuspendedUntil > DateTime.UtcNow)
            {
                throw AuthErrorException.AccountSuspended(user.SuspendedUntil.Value);
            }
            else if (user.SuspendedUntil != null)
            {
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
            throw;
        }
        catch (Exception)
        {
            throw AuthErrorException.LoginError();
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
        
        if (!context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            throw AuthErrorException.NoRefreshToken();
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
            
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "refresh")
            {
                throw AuthErrorException.InvalidTokenType();
            }
            
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw AuthErrorException.InvalidToken();
            }

            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                throw EntityNotFoundException.User(userId);
            }

            if (user.IsBanned)
            {
                throw AuthErrorException.UserBanned();
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);
            SetAuthCookies(context, newAccessToken, newRefreshToken);

            return new AuthPayload(user.Id, user.Name, user.Surname, user.Nickname, user.Email, user.ProfilePic, user.IsModerator);
        }
        catch (SecurityTokenExpiredException)
        {
            throw AuthErrorException.TokenExpired();
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            throw AuthErrorException.InvalidToken();
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
            expires: DateTime.UtcNow.AddMinutes(15),
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
            expires: DateTime.UtcNow.AddDays(7),
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
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
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
            SameSite = SameSiteMode.None
        };

        context.Response.Cookies.Delete("access_token", cookieOptions);
        context.Response.Cookies.Delete("refresh_token", cookieOptions);
    }

    public async Task<bool> ChangePassword(
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor,
        ChangePasswordInput input)
    {
        input.ValidateInput();

        var context = httpContextAccessor.HttpContext!;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) 
            ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new AuthenticationException();
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            throw EntityNotFoundException.User(userId);
        }

        if (!BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.Password))
        {
            throw AuthErrorException.InvalidPassword();
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
        await db.SaveChangesAsync();

        return true;
    }
}
