namespace SharedKernel.Abstractions;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// Call this whenever the entity is modified.
    /// EF Core value converters or the repository pattern should call this.
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}


// This abstract class represents an entity that supports soft deletion, 
//allowing it to be marked as deleted without being physically removed from the database. 
// It includes properties to track deletion status, deletion time, and who performed the deletion, 
//along with methods to perform soft delete and restore operations.
public abstract class SoftDeletableEntity : BaseEntity
{
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        SetUpdatedAt();
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        SetUpdatedAt();
    }
}

// This interface defines the structure for domain events, which are used to represent significant occurrences within the domain that other parts of the system may need to react to.
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

// This abstract record provides a base implementation for domain events, including properties for event identification and occurrence time, as well as an abstract property for the event type that must be implemented by derived classes.
public abstract record BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}


// This abstract class represents an aggregate root in the domain-driven design context, which is a cluster of related entities that are treated as a single unit for data changes. 
// It inherits from SoftDeletableEntity and includes a collection of domain events that can be raised and cleared, allowing for event-driven communication within the system.
public abstract class AggregateRoot : SoftDeletableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
