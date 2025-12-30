using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Common.Events;

/// <summary>
/// Abstraction for publishing domain events to external systems (SignalR, message bus, etc.)
/// Follows Dependency Inversion Principle - Application doesn't know about SignalR
/// </summary>
public interface IEventPublisher
{
    Task PublishTaskCreatedAsync(Guid taskId, Guid projectId, string title, CancellationToken cancellationToken = default);
    Task PublishTaskUpdatedAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default);
    Task PublishTaskStatusChangedAsync(Guid taskId, Guid projectId, ProjectTaskStatus newStatus, CancellationToken cancellationToken = default);
    Task PublishTaskDeletedAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default);
}