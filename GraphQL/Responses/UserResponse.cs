using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfilePic { get; set; }
    public string? BannerPic { get; set; }
    public DateTime Joined { get; set; }
    public bool IsModerator { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
    public string? BannedReason { get; set; }
    public List<UserSkillResponse> Skills { get; set; } = new();
    public List<UserInterestResponse> Interests { get; set; } = new();

    public static UserResponse FromUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Nickname = user.Nickname,
            Email = user.Email,
            ProfilePic = user.ProfilePic,
            BannerPic = user.BannerPic,
            Joined = user.Joined,
            IsModerator = user.IsModerator,
            IsSuspended = user.SuspendedUntil > DateTime.UtcNow,
            SuspendedUntil = user.SuspendedUntil,
            IsBanned = user.IsBanned,
            BannedUntil = user.BanExpiresAt,
            BannedReason = user.BanReason,
            Skills = user.Skills.Select(s => new UserSkillResponse { SkillName = s.SkillName }).ToList(),
            Interests = user.Interests.Select(i => new UserInterestResponse { InterestName = i.InterestName }).ToList()
        };
    }
}

public class UserSkillResponse
{
    public string SkillName { get; set; } = null!;
}

public class UserInterestResponse
{
    public string InterestName { get; set; } = null!;
}