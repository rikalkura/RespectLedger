using Ardalis.GuardClauses;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Domain.Entities;

public class User : BaseEntity<Guid>
{
    public string Nickname { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    
    public UserStatus Status { get; private set; }
    public UserRole Role { get; private set; }
    
    public int CurrentMana { get; private set; }
    public DateTime? LastManaReset { get; private set; }
    
    public int TotalXp { get; private set; }
    public int CurrentLevel { get; private set; }
    public string CurrentClass { get; private set; } = "Novice";
    
    // Navigation properties
    public ICollection<Respect> SentRespects { get; private set; } = new List<Respect>();
    public ICollection<Respect> ReceivedRespects { get; private set; } = new List<Respect>();
    public ICollection<UserAchievement> UserAchievements { get; private set; } = new List<UserAchievement>();
    
    private User() { } // EF Core constructor

    public User(string nickname, string email, string passwordHash)
    {
        Guard.Against.NullOrWhiteSpace(nickname, nameof(nickname));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.NullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.Against.StringTooLong(nickname, 50, nameof(nickname));
        Guard.Against.InvalidEmail(email, nameof(email));

        Id = Guid.NewGuid();
        Nickname = nickname;
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.Pending;
        Role = UserRole.User;
        CurrentMana = 3;
        LastManaReset = DateTime.UtcNow;
        TotalXp = 0;
        CurrentLevel = 1;
        CurrentClass = "Novice";
    }

    public void UpdateProfile(string? nickname, string? bio)
    {
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            Guard.Against.StringTooLong(nickname, 50, nameof(nickname));
            Nickname = nickname;
            MarkAsUpdated();
        }

        if (bio != null)
        {
            Guard.Against.StringTooLong(bio, 500, nameof(bio));
            Bio = bio;
            MarkAsUpdated();
        }
    }

    public void UpdateAvatar(string avatarUrl)
    {
        Guard.Against.NullOrWhiteSpace(avatarUrl, nameof(avatarUrl));
        AvatarUrl = avatarUrl;
        MarkAsUpdated();
    }

    public void Approve()
    {
        if (Status == UserStatus.Pending)
        {
            Status = UserStatus.Active;
            MarkAsUpdated();
        }
    }

    public void Ban()
    {
        Status = UserStatus.Banned;
        MarkAsUpdated();
    }

    public void ConsumeMana(int amount)
    {
        Guard.Against.NegativeOrZero(amount, nameof(amount));
        Guard.Against.OutOfRange(CurrentMana, nameof(CurrentMana), amount, int.MaxValue);
        
        CurrentMana -= amount;
        MarkAsUpdated();
    }

    public void ResetMana()
    {
        CurrentMana = 3;
        LastManaReset = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void ReceiveRespect(int xpGain = 1)
    {
        Guard.Against.NegativeOrZero(xpGain, nameof(xpGain));
        TotalXp += xpGain;
        MarkAsUpdated();
    }

    public void UpdateLevel(int level)
    {
        Guard.Against.NegativeOrZero(level, nameof(level));
        CurrentLevel = level;
        MarkAsUpdated();
    }

    public void UpdateClass(string className)
    {
        Guard.Against.NullOrWhiteSpace(className, nameof(className));
        CurrentClass = className;
        MarkAsUpdated();
    }

    public bool HasMana(int requiredAmount)
    {
        return CurrentMana >= requiredAmount;
    }

    public bool IsActive()
    {
        return Status == UserStatus.Active;
    }

    public bool IsAdmin()
    {
        return Role == UserRole.Admin;
    }
}
