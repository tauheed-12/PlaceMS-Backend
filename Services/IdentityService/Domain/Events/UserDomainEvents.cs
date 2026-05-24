using SharedKernel.Abstractions;

namespace IdentityService.Domain.Events;

public record UserCreatedDomainEvent(Guid UserId, string Email, string FullName, string Role) : BaseDomainEvent
{
    public override string EventType => "user.created";
}

public record UserEmailVerifiedDomainEvent(Guid UserId, string Email) : BaseDomainEvent
{
    public override string EventType => "user.email-verified";
}

public record PasswordResetRequestedDomainEvent(Guid UserId, string Email, string FullName, string ResetToken) : BaseDomainEvent
{
    public override string EventType => "user.password-reset-requested";
}

public record UserDeactivatedDomainEvent(Guid UserId, string Email, string DeactivatedBy) : BaseDomainEvent
{
    public override string EventType => "user.deactivated";
}