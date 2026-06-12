namespace SharedKernel.Messaging;

/// <summary>
/// Standard envelope for all Kafka messages across PMS services.
/// Every producer wraps their payload in this — consumers unwrap it.
/// Provides correlation, tracing, and versioning out of the box.
/// </summary>
public class MessageEnvelope<TPayload>
{
    /// <summary>Unique ID for this message — used for idempotency checks.</summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>Correlation ID for distributed tracing across services.</summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The service that produced this message.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Topic the message was published to.</summary>
    public string Topic { get; init; } = string.Empty;

    /// <summary>Event type name — matches KafkaTopics constants.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Schema version — increment when payload shape changes.</summary>
    public string SchemaVersion { get; init; } = "1.0";

    /// <summary>UTC timestamp of when the event occurred.</summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>The actual event payload.</summary>
    public TPayload Payload { get; init; } = default!;
}

// ─────────────────────────────────────────────────────
// Event Payloads — one record per Kafka topic
// These are the contracts between producer and consumer.
// ─────────────────────────────────────────────────────

/// <summary>pms.user.registered — fired by IdentityService after registration.</summary>
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string VerificationToken { get; init; } = string.Empty;
    public string VerificationLink { get; init; } = string.Empty;
}

/// <summary>pms.user.email-verification — fired to trigger verification email.</summary>
public record UserEmailVerificationEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string VerificationToken { get; init; } = string.Empty;
    public string VerificationLink { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty; // Plaintext password for initial welcome/verification email
}

/// <summary>pms.user.password-reset — fired to trigger password reset email.</summary>
public record UserPasswordResetEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
    public string ResetLink { get; init; } = string.Empty;
}

/// <summary>pms.college.registered — fired by CollegeService.</summary>
public record CollegeRegisteredEvent
{
    public Guid CollegeId { get; init; }
    public string CollegeEmail { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public Guid RegisteredByAdminId { get; init; }
}

public record CollegeActivatedEvent
{
    public Guid CollegeId { get; init; }
    public string CollegeEmail { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public Guid ByAdminId { get; init; }
}

public record CollegeDeactivatedEvent
{
    public Guid CollegeId { get; init; }
    public string CollegeEmail { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public Guid ByAdminId { get; init; }
}

/// <summary>pms.tpo.assigned — fired by CollegeService when TPO is assigned.</summary>
public record TpoAssignedEvent
{
    public Guid TpoUserId { get; init; }
    public string TpoEmail { get; init; } = string.Empty;
    public string TpoName { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
}

/// <summary>pms.coordinator.added — fired by CollegeService.</summary>
public record CoordinatorAddedEvent
{
    public Guid CoordinatorUserId { get; init; }
    public string CoordinatorEmail { get; init; } = string.Empty;
    public string CoordinatorName { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string VerificationToken { get; init; } = string.Empty;
}

/// <summary>pms.drive.approval-requested — fired by DriveService when recruiter targets a college.</summary>
public record DriveApprovalRequestedEvent
{
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public Guid TpoUserId { get; init; }
    public string TpoEmail { get; init; } = string.Empty;
    public string TpoName { get; init; } = string.Empty;
    public string DriveDeadline { get; init; } = string.Empty;
}

/// <summary>pms.drive.approved — fired by DriveService after TPO approves.</summary>
public record DriveApprovedEvent
{
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public Guid TpoUserId { get; init; }
    public string Note { get; init; } = string.Empty;
}

/// <summary>pms.drive.rejected — fired by DriveService after TPO rejects.</summary>
public record DriveRejectedEvent
{
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public Guid TpoUserId { get; init; }
    public string RejectionNote { get; init; } = string.Empty;
    public Guid RecruiterUserId { get; init; }
    public string RecruiterEmail { get; init; } = string.Empty;
}

/// <summary>pms.application.submitted — fired by ApplicationService.</summary>
public record ApplicationSubmittedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
}

/// <summary>pms.application.status-changed — fired by ApplicationService on every status update.</summary>
public record ApplicationStatusChangedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentEmail { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
}

/// <summary>pms.placement.confirmed — fired when student status is set to Placed/Offered.</summary>
public record PlacementConfirmedEvent
{
    public Guid StudentUserId { get; init; }
    public string StudentEmail { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string Ctc { get; init; } = string.Empty;
}