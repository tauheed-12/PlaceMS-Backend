using SharedKernel.Abstractions;

namespace ApplicationService.Application.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
