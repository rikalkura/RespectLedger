using Ardalis.GuardClauses;

namespace RespectLedger.Domain.Entities;

public class RespectLike : BaseEntity<int>
{
    public int RespectId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Respect Respect { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private RespectLike() { } // EF Core constructor

    public RespectLike(int respectId, Guid userId)
    {
        Guard.Against.NegativeOrZero(respectId, nameof(respectId));
        Guard.Against.Default(userId, nameof(userId));

        RespectId = respectId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
