using SharedKernel.Abstractions;

namespace CollegeService.Application.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}