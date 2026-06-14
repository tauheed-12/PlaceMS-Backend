namespace NotificationService.Domain.Enums;

/// <summary>
/// All notification event types across the PMS system.
/// Each type maps to an email template + in-app message template.
/// Adding a new notification = add enum value + template + handler.
/// </summary>
public enum NotificationType
{
    // ── Auth / Identity ───────────────────────────────────────────
    EmailVerification = 1,
    PasswordReset = 2,
    WelcomeStudent = 3,
    WelcomeTPO = 4,
    WelcomeCoordinator = 5,
    AccountDeactivated = 6,

    // ── Drive ─────────────────────────────────────────────────────
    DriveApprovalRequested = 10,    // → TPO
    DriveApproved = 11,             // → Recruiter
    DriveRejected = 12,             // → Recruiter
    DriveChangesRequested = 13,     // → Recruiter
    DriveResubmitted = 14,          // → TPO
    DriveDeactivated = 15,          // → Students who applied
    NewDriveAvailable = 16,         // → Students of approved college

    // ── Application ───────────────────────────────────────────────
    ApplicationSubmitted = 20,      // → Student (confirmation)
    ApplicationUnderReview = 21,    // → Student
    ApplicationShortlisted = 22,    // → Student
    ApplicationOffered = 23,        // → Student
    ApplicationRejected = 24,       // → Student
    ApplicationWithdrawn = 25,        // → Student
    PlacementConfirmed = 26,        // → Student + TPO,
    ApplicationStatusChanged = 27,        // → Student (generic status change notification)
}

/// <summary>
/// Delivery channel — a notification can be sent via multiple channels.
/// </summary>
public enum NotificationChannel
{
    Email = 1,
    InApp = 2
}

/// <summary>
/// Current state of a notification record.
/// </summary>
public enum NotificationStatus
{
    Pending = 1,        // Created, not yet sent
    Sent = 2,           // Successfully delivered
    Failed = 3,         // All retries exhausted
    Read = 4            // In-app only — user has read it
}