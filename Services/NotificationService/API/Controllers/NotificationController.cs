using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.DTOs.Responses;
using NotificationService.Application.Interfaces;
using SharedKernel.Extensions;
using SharedKernel.Models;
using SharedKernel.Wrappers;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
        => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unreadOnly = null,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _service.GetMyNotificationsAsync(userId, page, pageSize, unreadOnly, ct);
        return Ok(ApiResponse<PagedResult<NotificationResponse>>.Ok(result));
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), 200)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var count = await _service.GetUnreadCountAsync(userId, ct);
        return Ok(ApiResponse<UnreadCountResponse>.Ok(new UnreadCountResponse { Count = count }));
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _service.MarkAsReadAsync(userId, id, ct);
        return Ok(ApiResponse.Ok("Notification marked as read."));
    }

    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _service.MarkAllAsReadAsync(userId, ct);
        return Ok(ApiResponse.Ok("All notifications marked as read."));
    }

    [HttpGet("preferences")]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationPreferenceResponse>>), 200)]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _service.GetPreferencesAsync(userId, ct);
        return Ok(ApiResponse<List<NotificationPreferenceResponse>>.Ok(result));
    }

    [HttpPut("preferences")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> UpdatePreference(
        [FromBody] UpdatePreferenceRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _service.UpdatePreferenceAsync(userId, request, ct);
        return Ok(ApiResponse.Ok("Preference updated."));
    }
}