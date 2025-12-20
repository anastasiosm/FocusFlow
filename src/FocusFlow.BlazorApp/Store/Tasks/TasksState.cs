using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Enums;
using Fluxor;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// State for Tasks feature with filtering
/// </summary>
[FeatureState]
public record TasksState
{
	public List<TaskDto> Tasks { get; init; }
	public TaskDto? SelectedTask { get; init; }
	public bool IsLoading { get; init; }
	public bool IsLoadingTask { get; init; }
	public string? ErrorMessage { get; init; }
	
	// Filter properties
	public ProjectTaskStatus? StatusFilter { get; init; }
	public Priority? PriorityFilter { get; init; }
	public bool? IsOverdueFilter { get; init; }

	private TasksState()
	{
		Tasks = new List<TaskDto>();
	}

	public TasksState(List<TaskDto> tasks, TaskDto? selectedTask, bool isLoading, bool isLoadingTask, string? errorMessage, 
		ProjectTaskStatus? statusFilter = null, Priority? priorityFilter = null, bool? isOverdueFilter = null)
	{
		Tasks = tasks;
		SelectedTask = selectedTask;
		IsLoading = isLoading;
		IsLoadingTask = isLoadingTask;
		ErrorMessage = errorMessage;
		StatusFilter = statusFilter;
		PriorityFilter = priorityFilter;
		IsOverdueFilter = isOverdueFilter;
	}
}
