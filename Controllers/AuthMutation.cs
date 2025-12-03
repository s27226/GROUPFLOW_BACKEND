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
        return new AuthPayload(user.Id, user.Name, user.Email, token);
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

            var token = GenerateJwt(user);
            return new AuthPayload(user.Id, user.Name, user.Email, token);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.ToString());
        }
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
}
