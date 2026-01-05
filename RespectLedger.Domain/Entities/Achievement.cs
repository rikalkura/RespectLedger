using Ardalis.GuardClauses;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Domain.Entities;

public class Achievement : BaseEntity<int>
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    
    public AchievementCriteriaType CriteriaType { get; private set; }
    public int CriteriaValue { get; private set; }

    private Achievement() { } // EF Core constructor

    public Achievement(string title, AchievementCriteriaType criteriaType, int criteriaValue, string? description = null, string? iconUrl = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.StringTooLong(title, 100, nameof(title));
        Guard.Against.NegativeOrZero(criteriaValue, nameof(criteriaValue));

        Title = title;
        CriteriaType = criteriaType;
        CriteriaValue = criteriaValue;
        Description = description;
        IconUrl = iconUrl;
    }
}
