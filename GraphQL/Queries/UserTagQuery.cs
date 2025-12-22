using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class UserTagQuery
{
    [Authorize]
    [GraphQLName("myskills")]
    public async Task<List<UserSkill>> GetMySkills(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new List<UserSkill>();
        }

        return await context.UserSkills
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.SkillName)
            .ToListAsync();
    }

    [Authorize]
    [GraphQLName("myinterests")]
    public async Task<List<UserInterest>> GetMyInterests(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new List<UserInterest>();
        }

        return await context.UserInterests
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.InterestName)
            .ToListAsync();
    }
}
