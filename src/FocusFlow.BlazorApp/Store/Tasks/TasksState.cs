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
	public bool IsLoading { get; init; }
	public string? ErrorMessage { get; init; }
	
	// Filter properties
	public ProjectTaskStatus? StatusFilter { get; init; }
	public Priority? PriorityFilter { get; init; }
	public bool? IsOverdueFilter { get; init; }

	private TasksState()
	{
		Tasks = new List<TaskDto>();
	}

	public TasksState(List<TaskDto> tasks, bool isLoading, string? errorMessage, 
		ProjectTaskStatus? statusFilter = null, Priority? priorityFilter = null, bool? isOverdueFilter = null)
	{
		Tasks = tasks;
		IsLoading = isLoading;
		ErrorMessage = errorMessage;
		StatusFilter = statusFilter;
		PriorityFilter = priorityFilter;
		IsOverdueFilter = isOverdueFilter;
	}
}
