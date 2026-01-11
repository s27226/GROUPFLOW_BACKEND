using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Users.GraphQL.Inputs;

namespace GROUPFLOW.Features.Users.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for user skills and interests.
/// </summary>
public class UserTagMutation
{
    private readonly ILogger<UserTagMutation> _logger;

    public UserTagMutation(ILogger<UserTagMutation> logger)
    {
        _logger = logger;
    }

    [Authorize]
    [GraphQLName("addskill")]
    public async Task<UserSkill> AddSkill(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        UserSkillInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var existingSkill = await context.UserSkills
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SkillName == input.SkillName, ct);

        if (existingSkill != null)
            return existingSkill;

        var skill = new UserSkill
        {
            UserId = userId,
            SkillName = input.SkillName,
            AddedAt = DateTime.UtcNow
        };

        context.UserSkills.Add(skill);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} added skill: {SkillName}", userId, input.SkillName);
        return skill;
    }

    [Authorize]
    [GraphQLName("removeskill")]
    public async Task<bool> RemoveSkill(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int skillId,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var skill = await context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId, ct)
            ?? throw new EntityNotFoundException("UserSkill", skillId);

        context.UserSkills.Remove(skill);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} removed skill {SkillId}", userId, skillId);
        return true;
    }

    [Authorize]
    [GraphQLName("addinterest")]
    public async Task<UserInterest> AddInterest(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        UserInterestInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var existingInterest = await context.UserInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.InterestName == input.InterestName, ct);

        if (existingInterest != null)
            return existingInterest;

        var interest = new UserInterest
        {
            UserId = userId,
            InterestName = input.InterestName,
            AddedAt = DateTime.UtcNow
        };

        context.UserInterests.Add(interest);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} added interest: {InterestName}", userId, input.InterestName);
        return interest;
    }

    [Authorize]
    [GraphQLName("removeinterest")]
    public async Task<bool> RemoveInterest(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int interestId,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var interest = await context.UserInterests
            .FirstOrDefaultAsync(i => i.Id == interestId && i.UserId == userId, ct)
            ?? throw new EntityNotFoundException("UserInterest", interestId);

        context.UserInterests.Remove(interest);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} removed interest {InterestId}", userId, interestId);
        return true;
    }
}
