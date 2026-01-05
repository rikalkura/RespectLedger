using Ardalis.GuardClauses;

namespace RespectLedger.Domain.Entities;

public class UserAchievement : BaseEntity<int>
{
    public Guid UserId { get; private set; }
    public int AchievementId { get; private set; }
    public DateTime UnlockedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Achievement Achievement { get; private set; } = null!;

    private UserAchievement() { } // EF Core constructor

    public UserAchievement(Guid userId, int achievementId)
    {
        Guard.Against.Default(userId, nameof(userId));
        Guard.Against.NegativeOrZero(achievementId, nameof(achievementId));

        UserId = userId;
        AchievementId = achievementId;
        UnlockedAt = DateTime.UtcNow;
    }
}
