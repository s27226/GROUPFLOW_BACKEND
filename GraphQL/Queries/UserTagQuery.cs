using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class UserTagQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTagQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [Authorize]
    [GraphQLName("myskills")]
    public async Task<List<UserSkill>> GetMySkills()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new List<UserSkill>();
        }

        return await _context.UserSkills
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.SkillName)
            .ToListAsync();
    }

    [Authorize]
    [GraphQLName("myinterests")]
    public async Task<List<UserInterest>> GetMyInterests()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new List<UserInterest>();
        }

        return await _context.UserInterests
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.InterestName)
            .ToListAsync();
    }
}
