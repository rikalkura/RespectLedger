using Ardalis.GuardClauses;

namespace RespectLedger.Domain.Entities;

public class SeasonResult : BaseEntity<int>
{
    public int SeasonId { get; private set; }
    public Guid UserId { get; private set; }
    
    public int RankPosition { get; private set; }
    public int TotalScore { get; private set; }
    public string? RewardSummary { get; private set; }
    public DateTime RecordedAt { get; private set; }

    // Navigation properties
    public Season Season { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private SeasonResult() { } // EF Core constructor

    public SeasonResult(int seasonId, Guid userId, int rankPosition, int totalScore, string? rewardSummary = null)
    {
        Guard.Against.NegativeOrZero(seasonId, nameof(seasonId));
        Guard.Against.Default(userId, nameof(userId));
        Guard.Against.NegativeOrZero(rankPosition, nameof(rankPosition));
        Guard.Against.Negative(totalScore, nameof(totalScore));

        if (!string.IsNullOrWhiteSpace(rewardSummary))
        {
            Guard.Against.StringTooLong(rewardSummary, 255, nameof(rewardSummary));
        }

        SeasonId = seasonId;
        UserId = userId;
        RankPosition = rankPosition;
        TotalScore = totalScore;
        RewardSummary = rewardSummary;
        RecordedAt = DateTime.UtcNow;
    }
}
