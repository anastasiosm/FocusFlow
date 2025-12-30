using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace FocusFlow.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time task notifications.
/// Hub = Server-side endpoint that clients connect to.
/// 
/// KEY CONCEPTS:
/// - Clients connect to this hub via WebSocket
/// - Hub can send messages to specific clients, groups, or all clients
/// - Groups = logical grouping (e.g., all users viewing Project X)
/// </summary>
[Authorize]  // Require JWT authentication
public class TasksHub : Hub
{
    private readonly ILogger<TasksHub> _logger;

    public TasksHub(ILogger<TasksHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called automatically when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("🔌 SignalR client connected | ConnectionId: {ConnectionId} | User: {UserId}",
            connectionId, userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called automatically when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("🔌 SignalR client disconnected | ConnectionId: {ConnectionId}",
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this method to join a project-specific group.
    /// Only clients in the group will receive project-related updates.
    /// 
    /// WHY GROUPS?
    /// - User viewing Project A shouldn't get updates for Project B
    /// - Reduces network traffic
    /// - Better security (users only get data they should see)
    /// </summary>
    public async Task JoinProject(string projectId)
    {
        var groupName = $"project_{projectId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("👥 Client joined project group | ConnectionId: {ConnectionId} | ProjectId: {ProjectId}",
            Context.ConnectionId, projectId);
    }

    /// <summary>
    /// Client calls this to leave a project group (e.g., when navigating away)
    /// </summary>
    public async Task LeaveProject(string projectId)
    {
        var groupName = $"project_{projectId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("👥 Client left project group | ConnectionId: {ConnectionId} | ProjectId: {ProjectId}",
            Context.ConnectionId, projectId);
    }
}