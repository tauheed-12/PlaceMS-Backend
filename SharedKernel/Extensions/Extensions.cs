using System.Security.Claims;
using SharedClaimTypes = SharedKernel.Constants.CustomClaimTypes;
using SharedKernel.Exceptions;


namespace SharedKernel.Extensions;

/// <summary>
/// ClaimsPrincipal extensions — extract typed values from JWT claims.
/// Used in controllers to get the calling user's identity without boilerplate.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Gets the authenticated user's ID from JWT claims.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(SharedClaimTypes.UserId)
            ?? throw new UnauthorizedException("User ID claim not found in token.");

        return Guid.TryParse(claim.Value, out var id)
            ? id
            : throw new UnauthorizedException("Invalid user ID in token.");
    }

    /// <summary>Gets the user's role from JWT claims.</summary>
    public static string GetRole(this ClaimsPrincipal principal)
        => principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
           ?? throw new UnauthorizedException("Role claim not found in token.");

    /// <summary>Gets the user's email from JWT claims.</summary>
    public static string GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
           ?? throw new UnauthorizedException("Email claim not found in token.");

    /// <summary>Gets the college ID associated with the user (TPO, Coordinator, Student).</summary>
    public static Guid? GetCollegeId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(SharedClaimTypes.CollegeId);
        return Guid.TryParse(claim?.Value, out var id) ? id : null;
    }

    /// <summary>Gets the college code — used for student registration validation.</summary>
    public static string? GetCollegeCode(this ClaimsPrincipal principal)
        => principal.FindFirst(SharedClaimTypes.CollegeCode)?.Value;

    /// <summary>Checks if the user has a specific role.</summary>
    public static bool IsInRole(this ClaimsPrincipal principal, string role)
        => principal.GetRole().Equals(role, StringComparison.OrdinalIgnoreCase);

    /// <summary>Gets full name from JWT claims.</summary>
    public static string GetFullName(this ClaimsPrincipal principal)
        => principal.FindFirst(SharedClaimTypes.FullName)?.Value ?? "Unknown";
}

/// <summary>
/// String extensions for common validation and formatting.
/// </summary>
public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static string ToCollegeCode(this string value)
        => value.Trim().ToUpperInvariant().Replace(" ", "");

    public static string Truncate(this string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];

    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var name = parts[0];
        var domain = parts[1];
        var masked = name.Length <= 2
            ? name
            : name[..2] + new string('*', Math.Min(name.Length - 2, 4));
        return $"{masked}@{domain}";
    }
}

/// <summary>
/// DateTime extensions for consistent UTC handling.
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

    public static bool IsExpired(this DateTime expiryDate)
        => DateTime.UtcNow > expiryDate;

    public static bool IsExpired(this DateTime? expiryDate)
        => expiryDate.HasValue && DateTime.UtcNow > expiryDate.Value;
}

/// <summary>
/// IQueryable extensions for pagination — avoids copy-pasting in every repository.
/// </summary>
public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }
}