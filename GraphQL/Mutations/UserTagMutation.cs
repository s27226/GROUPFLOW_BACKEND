using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class UserTagMutation
{
    [Authorize]
    [GraphQLName("addskill")]
    public async Task<UserSkill?> AddSkill(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        UserSkillInput input)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        var existingSkill = await context.UserSkills
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SkillName == input.SkillName);

        if (existingSkill != null)
        {
            return existingSkill;
        }

        var skill = new UserSkill
        {
            UserId = userId,
            SkillName = input.SkillName,
            AddedAt = DateTime.UtcNow
        };

        context.UserSkills.Add(skill);
        await context.SaveChangesAsync();
        return skill;
    }

    [Authorize]
    [GraphQLName("removeskill")]
    public async Task<bool> RemoveSkill(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int skillId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return false;
        }

        var skill = await context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        if (skill == null)
        {
            return false;
        }

        context.UserSkills.Remove(skill);
        await context.SaveChangesAsync();
        return true;
    }

    [Authorize]
    [GraphQLName("addinterest")]
    public async Task<UserInterest?> AddInterest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        UserInterestInput input)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        var existingInterest = await context.UserInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.InterestName == input.InterestName);

        if (existingInterest != null)
        {
            return existingInterest;
        }

        var interest = new UserInterest
        {
            UserId = userId,
            InterestName = input.InterestName,
            AddedAt = DateTime.UtcNow
        };

        context.UserInterests.Add(interest);
        await context.SaveChangesAsync();
        return interest;
    }

    [Authorize]
    [GraphQLName("removeinterest")]
    public async Task<bool> RemoveInterest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int interestId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return false;
        }

        var interest = await context.UserInterests
            .FirstOrDefaultAsync(i => i.Id == interestId && i.UserId == userId);

        if (interest == null)
        {
            return false;
        }

        context.UserInterests.Remove(interest);
        await context.SaveChangesAsync();
        return true;
    }

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
