using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NAME_WIP_BACKEND.Services;

public class UserTagService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserTagService> _logger;

    public UserTagService(AppDbContext context, ILogger<UserTagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserSkill?> AddSkill(int userId, string skillName)
    {
        var existingSkill = await _context.UserSkills
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SkillName == skillName);

        if (existingSkill != null)
        {
            _logger.LogWarning("User {UserId} tried to add an existing skill '{SkillName}'", userId, skillName);
            return existingSkill;
        }

        var skill = new UserSkill
        {
            UserId = userId,
            SkillName = skillName,
            AddedAt = DateTime.UtcNow
        };

        _context.UserSkills.Add(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} added skill '{SkillName}'", userId, skillName);
        return skill;
    }

    public async Task<bool> RemoveSkill(int userId, int skillId)
    {
        var skill = await _context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        if (skill == null)
        {
            _logger.LogWarning("User {UserId} tried to remove non-existent skill with Id {SkillId}", userId, skillId);
            return false;
        }

        _context.UserSkills.Remove(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed skill '{SkillName}' (Id {SkillId})", userId, skill.SkillName, skillId);
        return true;
    }

    public async Task<UserInterest?> AddInterest(int userId, string interestName)
    {
        var existingInterest = await _context.UserInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.InterestName == interestName);

        if (existingInterest != null)
        {
            _logger.LogWarning("User {UserId} tried to add an existing interest '{InterestName}'", userId, interestName);
            return existingInterest;
        }

        var interest = new UserInterest
        {
            UserId = userId,
            InterestName = interestName,
            AddedAt = DateTime.UtcNow
        };

        _context.UserInterests.Add(interest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} added interest '{InterestName}'", userId, interestName);
        return interest;
    }

    public async Task<bool> RemoveInterest(int userId, int interestId)
    {
        var interest = await _context.UserInterests
            .FirstOrDefaultAsync(i => i.Id == interestId && i.UserId == userId);

        if (interest == null)
        {
            _logger.LogWarning("User {UserId} tried to remove non-existent interest with Id {InterestId}", userId, interestId);
            return false;
        }

        _context.UserInterests.Remove(interest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed interest '{InterestName}' (Id {InterestId})", userId, interest.InterestName, interestId);
        return true;
    }
}
