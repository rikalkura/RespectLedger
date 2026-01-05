using Ardalis.GuardClauses;

namespace RespectLedger.Domain.Entities;

public class Season : BaseEntity<int>
{
    public string Name { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Season() { } // EF Core constructor

    public Season(string name, DateTime startDate, DateTime endDate)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.StringTooLong(name, 100, nameof(name));
        Guard.Against.OutOfRange(endDate, nameof(endDate), startDate, DateTime.MaxValue);

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public bool IsCurrent()
    {
        DateTime now = DateTime.UtcNow;
        return IsActive && now >= StartDate && now <= EndDate;
    }
}
