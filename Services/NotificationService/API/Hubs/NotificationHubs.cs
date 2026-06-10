using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedKernel.Extensions;

namespace NotificationService.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
        => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserId().ToString();

        if (userId is null)
        {
            _logger.LogWarning("Unauthenticated connection attempt to NotificationHub.");
            Context.Abort();
            return;
        }

        // Add to personal group — allows targeted push by userId
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        _logger.LogInformation("User {UserId} connected to NotificationHub", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.GetUserId().ToString();
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        await base.OnDisconnectedAsync(exception);
    }
}