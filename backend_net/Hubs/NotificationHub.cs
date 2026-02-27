using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace backend_net.Hubs;

/// <summary>
/// SignalR hub for real-time notifications
/// Handles connection management and notification broadcasting
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var roleName = GetRoleName();
        
        if (userId.HasValue)
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            
            // Add user to role-based group
            if (!string.IsNullOrEmpty(roleName))
            {
                var roleGroupName = $"role_{roleName.ToLower().Replace(" ", "_")}";
                await Groups.AddToGroupAsync(Context.ConnectionId, roleGroupName);
                _logger.LogInformation("User {UserId} with role {Role} connected to NotificationHub. ConnectionId: {ConnectionId}", 
                    userId.Value, roleName, Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}", 
                    userId.Value, Context.ConnectionId);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var roleName = GetRoleName();
        
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            
            // Remove from role-based group
            if (!string.IsNullOrEmpty(roleName))
            {
                var roleGroupName = $"role_{roleName.ToLower().Replace(" ", "_")}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roleGroupName);
            }
            
            _logger.LogInformation("User {UserId} disconnected from NotificationHub. ConnectionId: {ConnectionId}", userId.Value, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a group for role-based notifications
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave a group
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    private int? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetRoleName()
    {
        return Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    }
}

