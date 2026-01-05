namespace RespectLedger.Application.Features.Respects.DTOs;

public record RespectDto
{
    public int Id { get; init; }
    public Guid SenderId { get; init; }
    public string SenderNickname { get; init; } = string.Empty;
    public string? SenderAvatarUrl { get; init; }
    public Guid ReceiverId { get; init; }
    public string ReceiverNickname { get; init; } = string.Empty;
    public string? ReceiverAvatarUrl { get; init; }
    public int SeasonId { get; init; }
    public string SeasonName { get; init; } = string.Empty;
    public int Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Tag { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
