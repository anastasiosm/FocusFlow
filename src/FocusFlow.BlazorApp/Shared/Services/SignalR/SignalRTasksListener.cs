using Fluxor;
using FocusFlow.BlazorApp.Shared.Models.SignalR;
using FocusFlow.BlazorApp.Features.Tasks.List.Store;
using FocusFlow.BlazorApp.Features.Projects.Detail.Store;

namespace FocusFlow.BlazorApp.Shared.Services.SignalR;

/// <summary>
/// 🌉 BRIDGE between SignalR events and Fluxor actions.
/// 
/// This is registered as a scoped service and instantiated once per user session.
/// It listens to SignalR events and converts them to Fluxor actions.
/// 
/// WHY THIS PATTERN?
/// - Keeps Fluxor as the single source of truth
/// - Maintains unidirectional data flow
/// - SignalR updates go through the same flow as user actions
/// - Enables time-travel debugging (Fluxor DevTools work!)
/// </summary>
public class SignalRTasksListener : IAsyncDisposable
{
    private readonly ISignalRService _signalRService;
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<SignalRTasksListener> _logger;

    public SignalRTasksListener(
        ISignalRService signalRService,
        IDispatcher dispatcher,
        ILogger<SignalRTasksListener> logger)
    {
        _signalRService = signalRService;
        _dispatcher = dispatcher;
        _logger = logger;

        // Subscribe to SignalR events
        _signalRService.OnTaskCreated += HandleTaskCreated;
        _signalRService.OnTaskUpdated += HandleTaskUpdated;
        _signalRService.OnTaskStatusChanged += HandleTaskStatusChanged;
        _signalRService.OnTaskDeleted += HandleTaskDeleted;

        _logger.LogInformation("🎧 SignalRTasksListener initialized and listening");
    }

    /// <summary>
    /// When SignalR says "task created", dispatch Fluxor actions for both TasksList and ProjectDetail.
    /// The effects will then fetch the full task data from API.
    /// </summary>
    private Task HandleTaskCreated(TaskCreatedNotification notification)
    {
        _logger.LogInformation("🔔 SignalR: Task created, dispatching Fluxor actions | TaskId: {TaskId}, ProjectId: {ProjectId}",
            notification.TaskId, notification.ProjectId);

        // Dispatch to TasksList (for /tasks page)
        _dispatcher.Dispatch(new TasksListActions.TaskCreatedFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        // Dispatch to ProjectDetail (for /projects/{id} page)
        _dispatcher.Dispatch(new TaskCreatedInProjectFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        return Task.CompletedTask;
    }

    private Task HandleTaskUpdated(TaskUpdatedNotification notification)
    {
        _logger.LogInformation("🔔 SignalR: Task updated, dispatching Fluxor actions | TaskId: {TaskId}, ProjectId: {ProjectId}",
            notification.TaskId, notification.ProjectId);

        // Dispatch to TasksList (for /tasks page)
        _dispatcher.Dispatch(new TasksListActions.TaskUpdatedFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        // Dispatch to ProjectDetail (for /projects/{id} page)
        _dispatcher.Dispatch(new TaskUpdatedInProjectFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        return Task.CompletedTask;
    }

    private Task HandleTaskStatusChanged(TaskStatusChangedNotification notification)
    {
        _logger.LogInformation("🔔 SignalR: Task status changed, dispatching Fluxor actions | TaskId: {TaskId}, ProjectId: {ProjectId}, NewStatus: {NewStatus}",
            notification.TaskId, notification.ProjectId, notification.NewStatus);

        // Dispatch to TasksList (for /tasks page)
        _dispatcher.Dispatch(new TasksListActions.TaskStatusChangedFromSignalRAction(
            notification.TaskId,
            notification.ProjectId,
            notification.NewStatus));

        // Dispatch to ProjectDetail (for /projects/{id} page)
        _dispatcher.Dispatch(new TaskStatusChangedInProjectFromSignalRAction(
            notification.TaskId,
            notification.ProjectId,
            notification.NewStatus));

        return Task.CompletedTask;
    }

    private Task HandleTaskDeleted(TaskDeletedNotification notification)
    {
        _logger.LogInformation("🔔 SignalR: Task deleted, dispatching Fluxor actions | TaskId: {TaskId}, ProjectId: {ProjectId}",
            notification.TaskId, notification.ProjectId);

        // Dispatch to TasksList (for /tasks page)    
        _dispatcher.Dispatch(new TasksListActions.TaskDeletedFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        // Dispatch to ProjectDetail (for /projects/{id} page)
        _dispatcher.Dispatch(new TaskDeletedInProjectFromSignalRAction(
            notification.TaskId,
            notification.ProjectId));

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Unsubscribe when service is disposed
        _signalRService.OnTaskCreated -= HandleTaskCreated;
        _signalRService.OnTaskUpdated -= HandleTaskUpdated;
        _signalRService.OnTaskStatusChanged -= HandleTaskStatusChanged;
        _signalRService.OnTaskDeleted -= HandleTaskDeleted;

        _logger.LogInformation("🎧 SignalRTasksListener disposed");
    }
}