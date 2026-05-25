using SharedKernel.Abstractions;
using SharedKernel.Enums;
using IdentityService.Domain.Events;
using SharedKernel.Exceptions;

namespace IdentityService.Domain.Entities;

public class User : AggregateRoot
{
    // ------ Core Identity Properties ------
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Unverified;
    public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;

    // ------ College Association (null for admin and super admin) ------
    public Guid? CollegeId { get; private set; }
    public string? CollegeCode { get; private set; }

    // ------ Email Verification ------
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    // ------ Password Reset ------
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // ------ Login Tracking ------
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LockoutUntil { get; private set; }

    // ------ Navigation Properties ------
    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // ------ EF Core Constructor ------
    public User() { }

    // ------ Factory Methods ------

    // Factory method to create a new user with the necessary properties and raise a domain event for user creation.
    // Always use this factory — never call new User() directly.
    public static User CreateUser(string fullName, string email, string passwordHash, string phoneNumber, UserRole role, Guid? collegeId, string? collegeCode)
    {
        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber.Trim(),
            Role = role,
            CollegeId = collegeId,
            CollegeCode = collegeCode?.ToUpperInvariant(),
            VerificationStatus = VerificationStatus.Unverified,
            AccountStatus = AccountStatus.Active
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.FullName, user.Email, user.Role.ToString()));

        return user;
    }

    // Generates a unique email verification token, sets its expiry time, and updates the user entity accordingly. This method is typically called when a new user is created or when a user requests to resend the verification email.
    public string GenerateEmailVerificationToken()
    {
        var token = GenerateSecureToken();
        EmailVerificationToken = token;
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        SetUpdatedAt();
        return token;
    }

    // Verifies the user's email by checking the provided token against the stored token and ensuring it has not expired. If the verification is successful, it updates the user's verification status and records the time of verification.
    public void VerifyEmail(string token)
    {
        if (VerificationStatus == VerificationStatus.Verified)
            throw new InvalidOperationDomainException("Email is already verified.");

        if (AccountStatus == AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("Account is deactivated.");

        if (EmailVerificationToken is null || EmailVerificationTokenExpiry is null)
            throw new DomainValidationException("No verification token found. Request a new one.");

        if (EmailVerificationToken != token)
            throw new DomainValidationException("Invalid verification token.");

        if (DateTime.UtcNow > EmailVerificationTokenExpiry)
            throw new DomainValidationException("Verification token has expired. Request a new one.");

        VerificationStatus = VerificationStatus.Verified;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
        SetUpdatedAt();

        RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email));
    }

    // Generates a unique password reset token, valid for 1 hours.
    public string GeneratePasswordResetToken()
    {
        var token = GenerateSecureToken();
        PasswordResetToken = token;
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        RaiseDomainEvent(new PasswordResetRequestedDomainEvent(Id, Email, FullName, token));
        SetUpdatedAt();
        return token;
    }

    public void ResetPassword(string token, string newPassowrdHash)
    {
        if (PasswordResetToken is null || PasswordResetTokenExpiry is null)
            throw new DomainValidationException("No password reset token found. Request a new one.");

        if (PasswordResetToken != token)
            throw new DomainValidationException("Invalid password reset token.");

        if (DateTime.UtcNow > PasswordResetTokenExpiry)
            throw new DomainValidationException("Password reset token has expired. Request a new one.");

        PasswordHash = newPassowrdHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        FailedLoginAttempts = 0;
        LockoutUntil = null;

        _refreshTokens.ForEach(rt => rt.Invalidate());

        SetUpdatedAt();
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutUntil = null;
        SetUpdatedAt();
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockoutUntil = DateTime.UtcNow.AddMinutes(15);
        }
        SetUpdatedAt();
    }

    public bool IsLockedOut()
    {
        return LockoutUntil.HasValue && LockoutUntil.Value > DateTime.UtcNow;
    }

    public bool CanLogin()
    {
        return !IsDeleted && AccountStatus == AccountStatus.Active && !IsLockedOut();
    }

    // Adds a new refresh token. Removes expired tokens automatically.
    public void AddRefreshToken(RefreshToken token)
    {
        // Clean up expired tokens
        _refreshTokens.RemoveAll(rt => rt.IsExpired());
        _refreshTokens.Add(token);
        SetUpdatedAt();
    }

    public RefreshToken RotateRefreshToken(string token, string ipAddress)
    {
        var existingToken = _refreshTokens.SingleOrDefault(rt => rt.Token == token);

        if (existingToken is null)
            throw new UnauthorizedException("Refresh token not found");

        if (existingToken.IsRevoked)
            throw new UnauthorizedException("Refresh token has beedn revoked");

        if (existingToken.IsExpired())
            throw new UnauthorizedException("Refresh token has been expired, Please login again");

        // Revoke old token
        existingToken.Revoke("Rotated", ipAddress);

        // Issue new token
        var newToken = RefreshToken.Create(Id, ipAddress);
        AddRefreshToken(newToken);

        return newToken;
    }

    // Revoke all refresh token for this user. Logout all devices (Manual Revocation)
    public void RevokeAllRefreshTokens(string reason = "Manual Revocation")
    {
        _refreshTokens.Where(rt => !rt.IsExpired() && !rt.IsRevoked)
            .ToList()
            .ForEach(rt => rt.Revoke(reason));

        SetUpdatedAt();
    }

    // ------------------- Profile Updates --------------
    public void UpdateFullName(string fullName)
    {
        FullName = fullName.Trim();
        SetUpdatedAt();
    }

    public void UpdatePhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber.Trim();
        SetUpdatedAt();
    }

    public void AssignCollege(Guid collegeId, string collegeCode)
    {
        CollegeId = collegeId;
        CollegeCode = collegeCode;
        SetUpdatedAt();
    }

    // ------------ Status Management --------------------
    public void Deactivate()
    {
        if (AccountStatus == AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("User is already deactivated");

        AccountStatus = AccountStatus.Deactivated;
        RevokeAllRefreshTokens("Account Deactivated");
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        if (AccountStatus != AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("User is not Deactivated");

        AccountStatus = AccountStatus.Active;
        SetUpdatedAt();
    }


    // ------ Generate secure token --------------
    public static string GenerateSecureToken()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}