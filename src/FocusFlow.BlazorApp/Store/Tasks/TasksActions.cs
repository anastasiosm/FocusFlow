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
}
