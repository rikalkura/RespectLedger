using Ardalis.GuardClauses;

namespace RespectLedger.Domain.Entities;

public class Respect : BaseEntity<int>
{
    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public int SeasonId { get; private set; }
    
    public int Amount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Tag { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation properties (will be configured in EF Core)
    public User Sender { get; private set; } = null!;
    public User Receiver { get; private set; } = null!;
    public Season Season { get; private set; } = null!;

    private Respect() { } // EF Core constructor

    public Respect(Guid senderId, Guid receiverId, int seasonId, string reason, string? tag = null, string? imageUrl = null)
    {
        Guard.Against.Default(senderId, nameof(senderId));
        Guard.Against.Default(receiverId, nameof(receiverId));
        Guard.Against.NegativeOrZero(seasonId, nameof(seasonId));
        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));
        Guard.Against.StringTooLong(reason, 280, nameof(reason));
        
        if (!string.IsNullOrWhiteSpace(tag))
        {
            Guard.Against.StringTooLong(tag, 50, nameof(tag));
        }

        SenderId = senderId;
        ReceiverId = receiverId;
        SeasonId = seasonId;
        Amount = 1; // Fixed value as per spec
        Reason = reason;
        Tag = tag;
        ImageUrl = imageUrl;
    }

    public void UpdateImage(string imageUrl)
    {
        Guard.Against.NullOrWhiteSpace(imageUrl, nameof(imageUrl));
        ImageUrl = imageUrl;
        MarkAsUpdated();
    }
}
