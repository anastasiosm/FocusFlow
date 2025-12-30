using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FocusFlow.Application.Common.Events;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Infrastructure.SignalR;

/// <summary>
/// Concrete implementation of IEventPublisher using SignalR.
/// This is where we actually send messages to connected clients.
/// 
/// ARCHITECTURE NOTE:
/// Application layer defines WHAT to publish (IEventPublisher interface)
/// Infrastructure layer defines HOW to publish (this implementation)
/// </summary>
public class SignalREventPublisher : IEventPublisher
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly ILogger<SignalREventPublisher> _logger;

    public SignalREventPublisher(
        IHubContext<Hub> hubContext,
        ILogger<SignalREventPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishTaskCreatedAsync(Guid taskId, Guid projectId, string title, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📡 Publishing TaskCreated event | TaskId: {TaskId} | ProjectId: {ProjectId}",
            taskId, projectId);

        var message = new TaskCreatedNotification
        {
            TaskId = taskId,
            ProjectId = projectId,
            Title = title,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{projectId}")
            .SendAsync("TaskCreated", message, cancellationToken);

        _logger.LogInformation("✅ TaskCreated event published successfully");
    }

    public async Task PublishTaskUpdatedAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📡 Publishing TaskUpdated event | TaskId: {TaskId} | ProjectId: {ProjectId}",
            taskId, projectId);

        // Create a strongly-typed message
        var message = new TaskUpdatedNotification
        {
            TaskId = taskId,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };

        // Send to ALL clients in the project group
        // This is the magic! All connected clients viewing this project will receive this message
        await _hubContext.Clients.Group($"project_{projectId}")
            .SendAsync("TaskUpdated", message, cancellationToken);

        _logger.LogInformation("✅ TaskUpdated event published successfully");
    }

    public async Task PublishTaskStatusChangedAsync(Guid taskId, Guid projectId, ProjectTaskStatus newStatus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📡 Publishing TaskStatusChanged event | TaskId: {TaskId} | Status: {Status}",
            taskId, newStatus);

        var message = new TaskStatusChangedNotification
        {
            TaskId = taskId,
            ProjectId = projectId,
            NewStatus = newStatus,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{projectId}")
            .SendAsync("TaskStatusChanged", message, cancellationToken);

        _logger.LogInformation("✅ TaskStatusChanged event published successfully");
    }

    public async Task PublishTaskDeletedAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📡 Publishing TaskDeleted event | TaskId: {TaskId}",
            taskId);

        var message = new TaskDeletedNotification
        {
            TaskId = taskId,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{projectId}")
            .SendAsync("TaskDeleted", message, cancellationToken);

        _logger.LogInformation("✅ TaskDeleted event published successfully");
    }
}

// Notification DTOs - shared between server and client
public class TaskCreatedNotification
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public class TaskUpdatedNotification
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public DateTime Timestamp { get; init; }
}

public class TaskStatusChangedNotification
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public ProjectTaskStatus NewStatus { get; init; }
    public DateTime Timestamp { get; init; }
}

public class TaskDeletedNotification
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public DateTime Timestamp { get; init; }
}