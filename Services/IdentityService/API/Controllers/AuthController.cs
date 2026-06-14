using IdentityService.Application.DTOs.Requests;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;
using System.Linq;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Responses.AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress(), ct);
        return Ok(ApiResponse<Application.DTOs.Responses.AuthResponse>.Ok(result, "Login successful."));
    }

    [HttpPost("client-credentials")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Responses.ServiceTokenResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> ClientCredentials(
        [FromBody] ClientCredentialsRequest request,
        CancellationToken ct)
    {
        var clientConfig = _configuration.GetSection("ServiceClients")
            .GetChildren()
            .FirstOrDefault(c => string.Equals(c["ClientId"], request.ClientId, StringComparison.OrdinalIgnoreCase));

        if (clientConfig is null)
            return Unauthorized(ApiResponse.Fail("Invalid client ID."));

        var configuredSecret = clientConfig["ClientSecret"];
        if (string.IsNullOrWhiteSpace(configuredSecret) || configuredSecret != request.ClientSecret)
            return Unauthorized(ApiResponse.Fail("Invalid client secret."));

        var result = await _authService.GetServiceTokenAsync(request, ct);
        return Ok(ApiResponse<Application.DTOs.Responses.ServiceTokenResponse>.Ok(
            result,
            "Service token issued successfully."
        ));
    }

    [HttpPost("register/student")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Responses.RegisterResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterStudentAsync(request, ct);

        return CreatedAtAction(
            nameof(RegisterStudent),
            ApiResponse<Application.DTOs.Responses.RegisterResponse>.Created(result)
        );
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Responses.VerifyEmailResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        var result = await _authService.VerifyEmailAsync(token, ct);

        return Ok(
            ApiResponse<Application.DTOs.Responses.VerifyEmailResponse>.Ok(result)
        );
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationEmailRequest request,
        CancellationToken ct)
    {
        await _authService.ResendVerificationEmailAsync(request, ct);

        return Ok(
            ApiResponse.Ok("If the email exists and is unverified, a verification link has been sent.")
        );
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);

        return Ok(
            ApiResponse.Ok("If the email exists, a password reset link has been sent.")
        );
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request, ct);

        return Ok(
            ApiResponse.Ok("Password reset successfully. You can now log in with your new password.")
        );
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.Responses.AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request, GetIpAddress(), ct);

        return Ok(
            ApiResponse<Application.DTOs.Responses.AuthResponse>.Ok(
                result,
                "Token refreshed successfully."
            )
        );
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);

        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _authService.LogoutAllDevicesAsync(userId, ct);

        return Ok(ApiResponse.Ok("Logged out from all devices."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _authService.ChangePasswordAsync(userId, request, ct);

        return Ok(ApiResponse.Ok("Password changed successfully. Please log in again."));
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"]
                .ToString()
                .Split(',')
                .First()
                .Trim();

        return HttpContext.Connection.RemoteIpAddress?
            .MapToIPv4()
            .ToString() ?? "unknown";
    }
}