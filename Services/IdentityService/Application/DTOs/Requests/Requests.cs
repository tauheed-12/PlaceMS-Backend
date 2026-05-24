using SharedKernel.Enums;

namespace IdentityService.Application.DTOs.Requests;

// Login request — used by all roles through a single endpoint.
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

// Registration request for Admin and SuperAdmin adding users to the system.
public record RegisterUserRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; }

    // Required only for TPO, Coordinator, Student
    public Guid? CollegeId { get; init; }
    public string? CollegeCode { get; init; }
}

// Student self-registration — they pick their college via college code.
public record StudentRegisterRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
}

public record VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}

public record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public record ResendVerificationEmailRequest
{
    public string Email { get; init; } = string.Empty;
}