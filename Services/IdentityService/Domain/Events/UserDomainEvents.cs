using SharedKernel.Abstractions;

namespace IdentityService.Domain.Events;

public record UserCreatedDomainEvent(Guid UserId, string Email, string FullName, string Role, string EmailVerificationToken, string EmailVerificationLink) : BaseDomainEvent
{
    public override string EventType => "user.created";
}

public record UserEmailVerificationDomainEvent(Guid UserId, string Email, string FullName, string Token, string Link) : BaseDomainEvent
{
    public override string EventType => "user.email-verification";
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