using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Actions for Tasks feature
/// </summary>
public static class TasksActions
{
	public record LoadTasks;
	public record LoadTasksSuccess(List<TaskDto> Tasks);
	public record LoadTasksFailure(string ErrorMessage);
	
	public record SetStatusFilter(ProjectTaskStatus? Status);
	public record SetPriorityFilter(Priority? Priority);
	public record SetOverdueFilter(bool? IsOverdue);
	public record ClearFilters;
	
	// Task Detail actions
	public record LoadTaskById(Guid TaskId);
	public record LoadTaskByIdSuccess(TaskDto Task);
	public record LoadTaskByIdFailure(string ErrorMessage);
	
	public record UpdateTask(Guid TaskId, string Title, string? Description, DateTime? DueDate, Priority Priority);
	public record UpdateTaskSuccess(TaskDto Task);
	public record UpdateTaskFailure(string ErrorMessage);
	
	public record DeleteTask(Guid TaskId);
	public record DeleteTaskSuccess(Guid TaskId);
	public record DeleteTaskFailure(string ErrorMessage);
	
	public record UpdateTaskStatus(Guid TaskId, ProjectTaskStatus Status);
	public record UpdateTaskStatusSuccess(TaskDto Task);
	public record UpdateTaskStatusFailure(string ErrorMessage);
}
