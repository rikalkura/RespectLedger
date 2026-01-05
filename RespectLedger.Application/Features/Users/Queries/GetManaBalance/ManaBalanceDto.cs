namespace RespectLedger.Application.Features.Users.Queries.GetManaBalance;

public record ManaBalanceDto
{
    public int CurrentMana { get; init; }
    public int MaxMana { get; init; } = 3;
    public DateTime? LastReset { get; init; }
    public DateTime? NextReset { get; init; }
}
