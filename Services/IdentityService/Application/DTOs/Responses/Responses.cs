using SharedKernel.Enums;

namespace IdentityService.Application.DTOs.Responses;

// Returned after successful login or token refresh.
public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiry { get; init; }
    public DateTime RefreshTokenExpiry { get; init; }
    public UserDto User { get; init; } = null!;
}

// Basic user info embedded in auth responses and returned by profile endpoints.
public record UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string VerificationStatus { get; init; } = string.Empty;
    public Guid? CollegeId { get; init; }
    public string? CollegeCode { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Returned after successful registration — prompts user to verify email.
public record RegisterResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Message { get; init; } = "Registration successful. Please check your email to verify your account.";
}

// Returned after email verification
public record VerifyEmailResponse
{
    public bool Verified { get; init; }
    public string Message { get; init; } = string.Empty;
}

// Service-to-service token response (no refresh token for services)
public record ServiceTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; } // Seconds
}
