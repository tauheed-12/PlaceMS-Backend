using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace SharedKernel.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Registered in every service's Program.cs — catches all unhandled exceptions,
/// maps them to the correct HTTP status code, and always returns ApiResponse format.
///
/// Never leaks stack traces or internal messages in production.
/// All exceptions are logged with full context for debugging.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly bool _isDevelopment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironmentAccessor envAccessor)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = envAccessor.IsDevelopment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;
        var (statusCode, response) = MapException(exception, correlationId);

        // Log with full context — always log the real exception
        LogException(exception, context, statusCode, correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private (HttpStatusCode, object) MapException(Exception exception, string correlationId)
    {
        return exception switch
        {
            // FluentValidation failures — 400
            ValidationException fluentEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(
                    "Validation failed.",
                    fluentEx.Errors.Select(e => e.ErrorMessage).ToList()
                )
            ),

            // Domain validation — 400
            DomainValidationException domainValEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail("Validation failed.", domainValEx.Errors)
            ),

            // Business rule violation — 400
            BusinessRuleException busEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(busEx.Message)
            ),

            // Not found — 404
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail(notFoundEx.Message)
            ),

            // Unauthorized — 401
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail(unauthorizedEx.Message)
            ),

            // Forbidden — 403
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse.Fail(forbiddenEx.Message)
            ),

            // Conflict — 409
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                ApiResponse.Fail(conflictEx.Message)
            ),

            // Invalid operation — 422
            InvalidOperationDomainException invalidOpEx => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse.Fail(invalidOpEx.Message)
            ),

            // Downstream service failure — 503
            ServiceUnavailableException serviceEx => (
                HttpStatusCode.ServiceUnavailable,
                ApiResponse.Fail($"Service temporarily unavailable. Please try again later. [{correlationId}]")
            ),

            // Catch-all — 500 (never expose details in production)
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse.Fail(
                    $"An unexpected error occurred. Please contact support with reference: {correlationId}",
                    _isDevelopment ? exception.Message : "Internal server error."
                )
            )
        };
    }

    private void LogException(Exception exception, HttpContext context, HttpStatusCode statusCode, string correlationId)
    {
        var logContext = new
        {
            CorrelationId = correlationId,
            Path = context.Request.Path.Value,
            Method = context.Request.Method,
            StatusCode = (int)statusCode,
            User = context.User?.Identity?.Name ?? "anonymous",
            ExceptionType = exception.GetType().Name
        };

        if (statusCode >= HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId} | Path: {Path} | Method: {Method} | User: {User}",
                correlationId, logContext.Path, logContext.Method, logContext.User);
        }
        else if (statusCode >= HttpStatusCode.BadRequest)
        {
            _logger.LogWarning(
                "Handled exception {ExceptionType}. CorrelationId: {CorrelationId} | Path: {Path} | StatusCode: {StatusCode}",
                logContext.ExceptionType, correlationId, logContext.Path, (int)statusCode);
        }
    }
}

/// <summary>
/// Accessor injected into middleware to check environment without coupling to IHostEnvironment.
/// </summary>
public interface IWebHostEnvironmentAccessor
{
    bool IsDevelopment { get; }
}

public class WebHostEnvironmentAccessor : IWebHostEnvironmentAccessor
{
    public WebHostEnvironmentAccessor(IWebHostEnvironment env)
        => IsDevelopment = env.EnvironmentName == "Development";

    public bool IsDevelopment { get; }
}

/// <summary>
/// Extension to register the middleware cleanly.
/// Every service's Program.cs calls: app.UseGlobalExceptionHandler();
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}