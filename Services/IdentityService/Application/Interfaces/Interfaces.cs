using IdentityService.Application.DTOs.Requests;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Domain.Entities;
using SharedKernel.Enums;

namespace IdentityService.Application.Interfaces;


// Primary interface for all authentication operations.
// mplemented by AuthService in the Application layer.
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<RegisterResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<RegisterResponse> RegisterStudentAsync(StudentRegisterRequest request, CancellationToken ct = default);
    Task<VerifyEmailResponse> VerifyEmailAsync(string token, CancellationToken ct = default);
    Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAllDevicesAsync(Guid userId, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<ServiceTokenResponse> GetServiceTokenAsync(ClientCredentialsRequest request, CancellationToken ct = default);
}

// JWT generation and validation.
// Implemented in Infrastructure — takes User entity, returns signed JWT.
public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateServiceToken(string clientId);
    // bool ValidateToken(string token, out Guid userId);
}

// Password hashing and verification.
// Implemented in Infrastructure using BCrypt.
public interface IPasswordService
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hashedPassword);
}

/// User repository interface — extends base repository with Identity-specific queries.
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByEmailWithTokensAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByVerificationTokenAsync(string token, CancellationToken ct = default);
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}


// Publishes domain events to Kafka after SaveChanges.
// Reads accumulated domain events from aggregates and publishes to correct topics.
public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<SharedKernel.Abstractions.IDomainEvent> events, CancellationToken ct = default);
}


// Validates that a college code exists in the College Service.
// Called during student registration — inter-service HTTP call.
public interface ICollegeServiceClient
{
    Task<CollegeValidationResult?> ValidateCollegeCodeAsync(string collegeCode, CancellationToken ct = default);
}


public record CollegeValidationResult
{
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsValid { get; init; }
    public AccountStatus AccountStatus { get; init; }
}