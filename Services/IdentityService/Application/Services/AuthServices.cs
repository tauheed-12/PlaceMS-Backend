using IdentityService.Application.DTOs.Requests;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using SharedKernel.Enums;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Services;

// Orchestrates all authentication flows.
// Pure application logic — no HTTP, no EF, no Kafka here.
// Delegates to domain entities for business rules and repositories for persistence.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly ICollegeServiceClient _collegeClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        IDomainEventPublisher eventPublisher,
        ICollegeServiceClient collegeClient,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _eventPublisher = eventPublisher;
        _collegeClient = collegeClient;
        _logger = logger;
    }

    // ── Login ───────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailWithTokensAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        // Lockout check
        if (user.IsLockedOut())
            throw new UnauthorizedException("Account is temporarily locked due to multiple failed attempts. Try again later.");

        // Unverified check
        if (user.VerificationStatus == VerificationStatus.Unverified)
            throw new UnauthorizedException("Please verify your email address before logging in.");

        // Deactivated check
        if (user.AccountStatus == AccountStatus.Deactivated)
            throw new UnauthorizedException("Your account has been deactivated. Contact support.");

        // Password verification
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _userRepository.SaveChangesAsync(ct);
            _logger.LogWarning("Failed login attempt for {Email} from {Ip}", request.Email, ipAddress);
            throw new UnauthorizedException("Invalid email or password.");
        }

        // Success
        user.RecordSuccessfulLogin();

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = RefreshToken.Create(user.Id, ipAddress);
        user.AddRefreshToken(refreshToken);

        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} logged in from {Ip}", user.Id, ipAddress);

        return BuildAuthResponse(accessToken, refreshToken, user);
    }

    // ── Register (Admin/SuperAdmin adding staff) ─────────────────────
    public async Task<RegisterResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, ct))
            throw new ConflictException("User", "email", request.Email);

        Guid? collegeId = request.CollegeId;
        string? collegeCode = request.CollegeCode;

        // Validate college exists if provided
        if (collegeId.HasValue && !string.IsNullOrWhiteSpace(collegeCode))
        {
            var college = await _collegeClient.ValidateCollegeCodeAsync(collegeCode, ct);
            if (college is null || !college.IsActive)
                throw new NotFoundException("College", collegeCode);

            if (college.CollegeId != collegeId.Value)
                throw new DomainValidationException("College ID and college code do not match.");

            collegeId = college.CollegeId;
        }

        var password = request.FullName.Split(' ').LastOrDefault() + "@" + new Random().Next(1000, 9999);

        var passwordHash = _passwordService.HashPassword(password);

        var user = User.CreateUser(
            request.FullName,
            request.Email,
            passwordHash,
            request.PhoneNumber,
            request.Role,
            collegeId,
            collegeCode);

        user.GenerateEmailVerificationToken(password);

        await _userRepository.AddAsync(user, ct);

        await _userRepository.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(user, ct);

        _logger.LogInformation("User {UserId} registered with role {Role}", user.Id, request.Role);

        return new RegisterResponse { UserId = user.Id, Email = user.Email };
    }

    // ── Student Self-Registration ────────────────────────────────────
    public async Task<RegisterResponse> RegisterStudentAsync(StudentRegisterRequest request, CancellationToken ct = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, ct))
            throw new ConflictException("User", "email", request.Email);

        // Validate college code against College Service
        var college = await _collegeClient.ValidateCollegeCodeAsync(request.CollegeCode, ct)
            ?? throw new NotFoundException($"College with code '{request.CollegeCode}' not found.");

        if (!college.IsActive)
            throw new BusinessRuleException("This college is not accepting registrations.");

        var password = request.Password;
        var passwordHash = _passwordService.HashPassword(password);

        var user = User.CreateUser(
            request.FullName,
            request.Email,
            passwordHash,
            request.PhoneNumber,
            UserRole.Student,
            college.CollegeId,
            college.CollegeCode);

        user.GenerateEmailVerificationToken(password);

        await _userRepository.AddAsync(user, ct);

        await _userRepository.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(user, ct);

        _logger.LogInformation("Student {UserId} registered for college {CollegeCode}", user.Id, request.CollegeCode);

        return new RegisterResponse { UserId = user.Id, Email = user.Email };
    }

    // ── Email Verification ───────────────────────────────────────────
    public async Task<VerifyEmailResponse> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByVerificationTokenAsync(token, ct)
            ?? throw new NotFoundException("Verification token is invalid or has already been used.");

        user.VerifyEmail(token);

        await _userRepository.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(user, ct);

        return new VerifyEmailResponse { Verified = true, Message = "Email verified successfully. You can now log in." };
    }

    public async Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request, CancellationToken ct = default)
    {
        // Always return success — never confirm whether email exists (security)
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || user.VerificationStatus == VerificationStatus.Verified) return;

        user.RegenerateEmailVerificationToken();
        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Resent verification email to {Email}", request.Email);
    }

    // ── Password Reset ───────────────────────────────────────────────
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        // Never reveal whether email exists
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null) return;

        if (user.VerificationStatus != VerificationStatus.Verified) return;

        var resetToken = user.GeneratePasswordResetToken();
        _logger.LogInformation("Forogot password token {ResetToken}", resetToken);
        await _userRepository.SaveChangesAsync(ct);

        // Publish to Kafka → Notification Service sends reset email
        // (Domain event approach here would also work, using direct publish for simplicity)
        _logger.LogInformation("Password reset requested for {Email}", request.Email);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, ct)
            ?? throw new NotFoundException("Reset token is invalid or has expired.");

        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.ResetPassword(request.Token, newPasswordHash);

        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
    }

    // ── Token Refresh ────────────────────────────────────────────────
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, CancellationToken ct = default)
    {
        // We need to find which user owns this refresh token
        // In production, you'd store userId alongside the token or use a lookup table
        // Here we query via the token value
        var user = await FindUserByRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        if (user.VerificationStatus != VerificationStatus.Verified)
            throw new UnauthorizedException("Account is not active.");

        var newRefreshToken = user.RotateRefreshToken(request.RefreshToken, ipAddress);
        var accessToken = _jwtService.GenerateAccessToken(user);

        await _userRepository.SaveChangesAsync(ct);

        return BuildAuthResponse(accessToken, newRefreshToken, user);
    }

    // ── Logout ───────────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await FindUserByRefreshTokenAsync(refreshToken, ct);
        if (user is null) return; // Idempotent — no error if token not found

        var token = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);
        token?.Revoke("User logged out");

        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task LogoutAllDevicesAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdWithTokensAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        user.RevokeAllRefreshTokens("Logout all devices");
        await _userRepository.SaveChangesAsync(ct);
    }

    // ── Change Password ──────────────────────────────────────────────
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdWithTokensAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new DomainValidationException("Current password is incorrect.");

        var newHash = _passwordService.HashPassword(request.NewPassword);
        user.ResetPassword(user.PasswordResetToken ?? string.Empty, newHash);

        // For ChangePassword we don't need the reset token flow,
        // just update hash and invalidate sessions
        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Password changed for user {UserId}", userId);
    }

    // ── OAuth 2.0 Client Credentials (Service-to-Service) ────────────
    public Task<ServiceTokenResponse> GetServiceTokenAsync(ClientCredentialsRequest request, CancellationToken ct = default)
    {
        // Validate client credentials
        if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ClientSecret))
            throw new UnauthorizedException("Invalid client credentials.");

        // In production, you'd validate against a stored client credentials table
        // For now, we'll validate from configuration (see AuthController for config usage)
        // The controller will pass the configuration-validated clientId here

        // Generate service token with short expiry (15 minutes)
        var token = _jwtService.GenerateServiceToken(request.ClientId);

        _logger.LogInformation("Service token issued for client {ClientId}", request.ClientId);

        return Task.FromResult(new ServiceTokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 15 * 60 // 15 minutes in seconds
        });
    }

    // ── Private Helpers ──────────────────────────────────────────────
    private async Task PublishDomainEventsAsync(User user, CancellationToken ct)
    {
        var events = user.DomainEvents.ToList();
        if (events.Count == 0)
            return;

        await _eventPublisher.PublishAsync(events, ct);
        user.ClearDomainEvents();
    }

    private async Task<User?> FindUserByRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        // This requires a custom repo query since EF needs to traverse the token collection
        return await _userRepository.GetByIdWithTokensAsync(Guid.Empty, ct); // placeholder — see repo impl
    }

    private static AuthResponse BuildAuthResponse(string accessToken, RefreshToken refreshToken, User user)
        => new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiry = refreshToken.ExpiresAt,
            User = new()
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = user.Role.ToString(),
                VerificationStatus = user.VerificationStatus.ToString(),
                CollegeId = user.CollegeId,
                CollegeCode = user.CollegeCode,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            }
        };
}