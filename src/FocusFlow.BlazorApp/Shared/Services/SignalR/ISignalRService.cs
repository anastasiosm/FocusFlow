using FocusFlow.BlazorApp.Shared.Models.SignalR;

namespace FocusFlow.BlazorApp.Shared.Services.SignalR;

/// <summary>
/// Abstraction for SignalR client functionality.
/// 
/// RESPONSIBILITIES:
/// - Manage connection lifecycle (connect, disconnect, reconnect)
/// - Join/leave project groups
/// - Expose events for incoming messages
/// </summary>
public interface ISignalRService
{
    // Connection management
    Task StartAsync();
    Task StopAsync();
    bool IsConnected { get; }

    // Group management
    Task JoinProjectAsync(Guid projectId);
    Task LeaveProjectAsync(Guid projectId);

    // Events - components/services subscribe to these
    event Func<TaskCreatedNotification, Task>? OnTaskCreated;
    event Func<TaskUpdatedNotification, Task>? OnTaskUpdated;
    event Func<TaskStatusChangedNotification, Task>? OnTaskStatusChanged;
    event Func<TaskDeletedNotification, Task>? OnTaskDeleted;
}