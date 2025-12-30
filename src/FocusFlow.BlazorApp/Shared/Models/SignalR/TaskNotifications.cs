using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Shared.Models.SignalR;

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