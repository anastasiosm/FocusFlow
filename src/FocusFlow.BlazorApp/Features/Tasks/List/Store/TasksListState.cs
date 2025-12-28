using FocusFlow.Domain.Enums;
using Fluxor;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.List.Store;

/// <summary>
/// State for Tasks feature with filtering
/// </summary>
[FeatureState]
public record TasksListState
{
	public List<TaskResponse> Tasks { get; init; }
	public TaskResponse? SelectedTask { get; init; }
	public bool IsLoading { get; init; }
	public bool IsLoadingTask { get; init; }
	public string? ErrorMessage { get; init; }
	
	// Filter properties
	public ProjectTaskStatus? StatusFilter { get; init; }
	public Priority? PriorityFilter { get; init; }
	public bool? IsOverdueFilter { get; init; }

	private TasksListState()
	{
		Tasks = new List<TaskResponse>();
	}

	public TasksListState(List<TaskResponse> tasks, TaskResponse? selectedTask, bool isLoading, bool isLoadingTask, string? errorMessage, 
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
