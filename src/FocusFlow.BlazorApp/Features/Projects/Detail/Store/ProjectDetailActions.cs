using FocusFlow.BlazorApp.Features.Projects.Detail.Models;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

public record LoadProjectDetailAction(Guid ProjectId);
public record LoadProjectDetailSuccessAction(ProjectDetailDto Project);
public record LoadProjectDetailFailureAction(string Error);

/// <summary>
/// Clears error message from state
/// </summary>
public record ClearProjectDetailErrorAction();

// SignalR Actions for ProjectDetail
/// <summary>
/// Action dispatched when SignalR notifies that a task was created in this project
/// </summary>
public record TaskCreatedInProjectFromSignalRAction(Guid TaskId, Guid ProjectId);

/// <summary>
/// Action dispatched when SignalR notifies that a task was updated in this project
/// </summary>
public record TaskUpdatedInProjectFromSignalRAction(Guid TaskId, Guid ProjectId);

/// <summary>
/// Action dispatched when SignalR notifies that a task status changed in this project
/// </summary>
public record TaskStatusChangedInProjectFromSignalRAction(Guid TaskId, Guid ProjectId, ProjectTaskStatus NewStatus);

/// <summary>
/// Action dispatched when SignalR notifies that a task was deleted from this project
/// </summary>
public record TaskDeletedInProjectFromSignalRAction(Guid TaskId, Guid ProjectId);

// Success actions for when we fetch the updated task data
public record AddTaskToProjectSuccessAction(TaskResponse Task);
public record UpdateTaskInProjectSuccessAction(TaskResponse Task);
public record RemoveTaskFromProjectSuccessAction(Guid TaskId);
