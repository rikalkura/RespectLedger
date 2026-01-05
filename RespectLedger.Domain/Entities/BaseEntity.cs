namespace RespectLedger.Domain;

public abstract class BaseEntity<TId> : IEntity
{
    public TId Id { get; set; }
}

public interface IEntity { }
