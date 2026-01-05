using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class UserTagMutation
{
    private readonly UserTagService _service;

    public UserTagMutation(UserTagService service)
    {
        _service = service;
    }

    [Authorize]
    [GraphQLName("addskill")]
    public Task<UserSkill?> AddSkill(
        ClaimsPrincipal claimsPrincipal,
        UserSkillInput input)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.AddSkill(userId, input.SkillName);
    }

    [Authorize]
    [GraphQLName("removeskill")]
    public Task<bool> RemoveSkill(
        ClaimsPrincipal claimsPrincipal,
        int skillId)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.RemoveSkill(userId, skillId);
    }

    [Authorize]
    [GraphQLName("addinterest")]
    public Task<UserInterest?> AddInterest(
        ClaimsPrincipal claimsPrincipal,
        UserInterestInput input)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.AddInterest(userId, input.InterestName);
    }

    [Authorize]
    [GraphQLName("removeinterest")]
    public Task<bool> RemoveInterest(
        ClaimsPrincipal claimsPrincipal,
        int interestId)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.RemoveInterest(userId, interestId);
    }
}
