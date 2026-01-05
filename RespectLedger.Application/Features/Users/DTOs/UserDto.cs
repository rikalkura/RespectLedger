namespace RespectLedger.Application.Features.Users.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int CurrentMana { get; init; }
    public int TotalXp { get; init; }
    public int CurrentLevel { get; init; }
    public string CurrentClass { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
