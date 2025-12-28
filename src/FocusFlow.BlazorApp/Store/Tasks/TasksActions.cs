using FocusFlow.BlazorApp.Models.Dtos;
using FocusFlow.BlazorApp.Models.Tasks;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Actions for Tasks feature - Fluxor state management
/// All actions follow the pattern: {Verb}{Noun}[Success|Failure]Action
/// </summary>
public static class TasksActions
{
	// ============================================================================
	// Load Tasks (with filters)
	// ============================================================================

	/// <summary>
	/// Triggers loading of tasks with current filters from state
	/// </summary>
	public record LoadTasksAction();

	/// <summary>
	/// Dispatched when tasks are successfully loaded
	/// </summary>
	public record LoadTasksSuccessAction(List<TaskDto> Tasks);

	/// <summary>
	/// Dispatched when task loading fails
	/// </summary>
	public record LoadTasksFailureAction(string ErrorMessage);

	// ============================================================================
	// Filter Actions
	// ============================================================================

	/// <summary>
	/// Sets the status filter and triggers reload
	/// </summary>
	public record SetStatusFilterAction(ProjectTaskStatus? Status);

	/// <summary>
	/// Sets the priority filter and triggers reload
	/// </summary>
	public record SetPriorityFilterAction(Priority? Priority);

	/// <summary>
	/// Sets the overdue filter and triggers reload
	/// </summary>
	public record SetOverdueFilterAction(bool? IsOverdue);

	/// <summary>
	/// Clears all filters and triggers reload
	/// </summary>
	public record ClearFiltersAction();

	// ============================================================================
	// Load Single Task
	// ============================================================================

	/// <summary>
	/// Triggers loading of a single task by ID
	/// </summary>
	public record LoadTaskByIdAction(Guid TaskId);

	/// <summary>
	/// Dispatched when task is successfully loaded
	/// </summary>
	public record LoadTaskByIdSuccessAction(TaskDto Task);

	/// <summary>
	/// Dispatched when task loading fails
	/// </summary>
	public record LoadTaskByIdFailureAction(string ErrorMessage);

	// ============================================================================
	// Update Task
	// ============================================================================

	/// <summary>
	/// Triggers task update from edit result (from dialog)
	/// </summary>
	public record UpdateTaskFromResultAction(Guid TaskId, TaskEditResult EditResult);

	/// <summary>
	/// Dispatched when task is successfully updated
	/// </summary>
	public record UpdateTaskSuccessAction(TaskDto Task);

	/// <summary>
	/// Dispatched when task update fails
	/// </summary>
	public record UpdateTaskFailureAction(string ErrorMessage);

	// ============================================================================
	// Update Task Status (Quick action from dropdown)
	// ============================================================================

	/// <summary>
	/// Triggers quick status update (e.g., from dropdown in task list)
	/// </summary>
	public record UpdateTaskStatusAction(Guid TaskId, ProjectTaskStatus Status);

	/// <summary>
	/// Dispatched when task status is successfully updated
	/// </summary>
	public record UpdateTaskStatusSuccessAction(TaskDto Task);

	/// <summary>
	/// Dispatched when task status update fails
	/// </summary>
	public record UpdateTaskStatusFailureAction(string ErrorMessage);

	// ============================================================================
	// Delete Task
	// ============================================================================

	/// <summary>
	/// Triggers task deletion
	/// </summary>
	public record DeleteTaskAction(Guid TaskId);

	/// <summary>
	/// Dispatched when task is successfully deleted
	/// </summary>
	public record DeleteTaskSuccessAction(Guid TaskId);

	/// <summary>
	/// Dispatched when task deletion fails
	/// </summary>
	public record DeleteTaskFailureAction(string ErrorMessage);
}