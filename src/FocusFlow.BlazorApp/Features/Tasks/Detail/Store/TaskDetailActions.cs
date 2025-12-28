using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;
using FocusFlow.BlazorApp.Features.Tasks.Edit.Models;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Tasks.Detail.Store;

/// <summary>
/// Actions for Task Detail feature - Fluxor state management
/// All actions follow the pattern: {Verb}{Noun}[Success|Failure]Action
/// </summary>
public static class TaskDetailActions
{
	/// <summary>
	/// Triggers loading of a single task by ID
	/// </summary>
	public record LoadTaskByIdAction(Guid TaskId);
	/// <summary>
	/// Dispatched when a single task is successfully loaded
	/// </summary>
	public record LoadTaskByIdSuccessAction(TaskResponse Task);
	/// <summary>
	/// Dispatched when loading a single task fails
	/// </summary>
	public record LoadTaskByIdFailureAction(string ErrorMessage);

	// ============================================================================
	// Update Task Status
	// ============================================================================

	/// <summary>
	/// Triggers quick status update for the current task
	/// </summary>
	public record UpdateTaskStatusAction(Guid TaskId, ProjectTaskStatus Status);
	/// <summary>
	/// Dispatched when task status is successfully updated
	/// </summary>
	public record UpdateTaskStatusSuccessAction();
	/// <summary>
	/// Dispatched when task status update fails
	/// </summary>
	public record UpdateTaskStatusFailureAction(string ErrorMessage);

	// ============================================================================
	// Update Task (Full Update)
	// ============================================================================

	/// <summary>
	/// Triggers full task update from edit result
	/// </summary>
	public record UpdateTaskAction(Guid TaskId, TaskEditResult EditResult);
	/// <summary>
	/// Dispatched when task is successfully updated
	/// </summary>
	public record UpdateTaskSuccessAction();
	/// <summary>
	/// Dispatched when task update fails
	/// </summary>
	public record UpdateTaskFailureAction(string ErrorMessage);
}
