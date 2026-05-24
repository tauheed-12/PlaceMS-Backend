namespace SharedKernel.Exceptions;

/// <summary>
/// Base exception for all PMS domain and application exceptions.
/// The global exception handler uses this hierarchy to map to HTTP status codes.
/// </summary>
public abstract class PmsException : Exception
{
    public string ErrorCode { get; }

    protected PmsException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// 404 Not Found
/// Thrown when a requested resource does not exist.
/// </summary>
public class NotFoundException : PmsException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.", "NOT_FOUND") { }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND") { }
}

/// <summary>
/// 400 Bad Request — validation failures from domain logic.
/// FluentValidation failures use the ValidationException from
/// FluentValidation itself; this is for domain-level business rule violations.
/// </summary>
public class DomainValidationException : PmsException
{
    public List<string> Errors { get; }

    public DomainValidationException(string message)
        : base(message, "VALIDATION_FAILED")
    {
        Errors = new List<string> { message };
    }

    public DomainValidationException(List<string> errors)
        : base("One or more validation errors occurred.", "VALIDATION_FAILED")
    {
        Errors = errors;
    }
}

/// <summary>
/// 401 Unauthorized
/// Thrown when a user is not authenticated.
/// </summary>
public class UnauthorizedException : PmsException
{
    public UnauthorizedException(string message = "Authentication is required to access this resource.")
        : base(message, "UNAUTHORIZED") { }
}

/// <summary>
/// 403 Forbidden
/// Thrown when a user is authenticated but lacks permission.
/// </summary>
public class ForbiddenException : PmsException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, "FORBIDDEN") { }
}

/// <summary>
/// 409 Conflict
/// Thrown when a resource already exists (duplicate college code, email, etc.)
/// </summary>
public class ConflictException : PmsException
{
    public ConflictException(string resourceName, string conflictingField, object value)
        : base($"{resourceName} with {conflictingField} '{value}' already exists.", "CONFLICT") { }

    public ConflictException(string message)
        : base(message, "CONFLICT") { }
}

/// <summary>
/// 422 Unprocessable Entity
/// Thrown when the state transition is invalid (e.g., approving an already-approved drive).
/// </summary>
public class InvalidOperationDomainException : PmsException
{
    public InvalidOperationDomainException(string message)
        : base(message, "INVALID_OPERATION") { }
}

/// <summary>
/// 503 Service Unavailable
/// Thrown when a downstream service call fails.
/// </summary>
public class ServiceUnavailableException : PmsException
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName, string? detail = null)
        : base($"The '{serviceName}' service is currently unavailable. {detail}", "SERVICE_UNAVAILABLE")
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// 400 Bad Request — specifically for business rule violations.
/// Distinct from validation so handlers can log them differently.
/// </summary>
public class BusinessRuleException : PmsException
{
    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION") { }
}