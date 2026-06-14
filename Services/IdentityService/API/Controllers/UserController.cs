using IdentityService.Application.DTOs.Requests;
using IdentityService.Application.DTOs.Responses;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public UsersController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }


    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RegisterUserAsync(request, ct);
        return CreatedAtAction(nameof(GetUserById),
            new { id = result.UserId },
            ApiResponse<RegisterResponse>.Created(result));
    }


    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, ct);

        if (user is null)
            return NotFound(ApiResponse.Fail("User not found."));

        return Ok(ApiResponse<UserDto>.Ok(MapToDto(user)));
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);

        if (user is null)
            return NotFound(ApiResponse.Fail($"User with ID '{id}' not found."));

        return Ok(ApiResponse<UserDto>.Ok(MapToDto(user)));
    }

    private static UserDto MapToDto(Domain.Entities.User user) => new()
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
    };
}