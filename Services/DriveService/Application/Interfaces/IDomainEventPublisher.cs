using SharedKernel.Abstractions;

namespace DriveService.Application.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
