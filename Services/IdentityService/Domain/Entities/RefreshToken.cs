using SharedKernel.Abstractions;

namespace IdentityService.Domain.Entities;

// Refresh token owned by a User.
// Stored in the database — allows token rotation and revocation.
// EF Core maps this as an owned entity collection on User.
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? CreatedByIp { get; private set; }

    // EF Core constructor
    private RefreshToken() { }

    //Creates a new refresh token valid for 7 days.
    public static RefreshToken Create(Guid userId, string? ipAddress = null)
        => new()
        {
            UserId = userId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };


    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired();

    public void Revoke(string reason, string? byIp = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        RevokedByIp = byIp;
    }

    public void Invalidate()
    {
        Revoke("Invalidated due to password change or manual revocation");
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        Guid.NewGuid().ToByteArray().CopyTo(bytes, 0);
        Guid.NewGuid().ToByteArray().CopyTo(bytes, 16);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}