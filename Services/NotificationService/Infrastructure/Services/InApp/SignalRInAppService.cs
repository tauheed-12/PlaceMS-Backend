using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs.Requests;
using NotificationService.Application.Interfaces;
using NotificationService.API.Hubs;

namespace NotificationService.Infrastructure.Services.InApp;

public class SignalRInAppService : IInAppNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRInAppService> _logger;

    public SignalRInAppService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRInAppService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushAsync(Guid userId, InAppNotificationPayload payload, CancellationToken ct = default)
    {
        // Each user joins a group named by their userId on connect
        // So pushing to group = pushing to all connected devices of that user
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", payload, ct);

        _logger.LogDebug("Pushed in-app notification {NotificationId} to user {UserId}",
            payload.NotificationId, userId);
    }
}