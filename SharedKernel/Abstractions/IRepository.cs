namespace SharedKernel.Abstractions;

/// <summary>
/// Generic repository interface.
/// Each service's infrastructure layer implements this for each aggregate.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

/// <summary>
/// Unit of Work — wraps a single database transaction.
/// Call SaveChangesAsync after all repository operations to commit atomically.
/// Also dispatches any accumulated domain events from aggregates.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for Kafka message producers.
/// Each service has its own producer implementations.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}

/// <summary>
/// Marker interface for Kafka message consumers.
/// Registered in the DI container and started as hosted services.
/// </summary>
public interface IEventConsumer
{
    Task ConsumeAsync(CancellationToken cancellationToken = default);
}