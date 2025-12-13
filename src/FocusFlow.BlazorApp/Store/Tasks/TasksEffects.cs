using Fluxor;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Effects for Tasks feature
/// </summary>
public class TasksEffects
{
	private readonly IApiService _apiService;
	private readonly ILogger<TasksEffects> _logger;
	private readonly IState<TasksState> _state;

	public TasksEffects(IApiService apiService, ILogger<TasksEffects> logger, IState<TasksState> state)
	{
		_apiService = apiService;
		_logger = logger;
		_state = state;
	}

	[EffectMethod]
	public async Task HandleLoadTasks(TasksActions.LoadTasks action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Loading tasks with filters...");

		// Get filters from current state
		var status = _state.Value.StatusFilter;
		var priority = _state.Value.PriorityFilter;
		var isOverdue = _state.Value.IsOverdueFilter;

		var result = await _apiService.GetTasksFilteredAsync(status, priority, isOverdue);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully loaded {Count} tasks", result.Data?.Count ?? 0);
			dispatcher.Dispatch(new TasksActions.LoadTasksSuccess(result.Data ?? new()));
		}
		else
		{
			_logger.LogError("Failed to load tasks: {Error}", result.Error);
			dispatcher.Dispatch(new TasksActions.LoadTasksFailure(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleSetStatusFilter(TasksActions.SetStatusFilter action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Applying status filter: {Status}", action.Status);
		
		// Filter is already set in reducer, now reload tasks
		await Task.CompletedTask;
		dispatcher.Dispatch(new TasksActions.LoadTasks());
	}

	[EffectMethod]
	public async Task HandleSetPriorityFilter(TasksActions.SetPriorityFilter action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Applying priority filter: {Priority}", action.Priority);
		
		// Filter is already set in reducer, now reload tasks
		await Task.CompletedTask;
		dispatcher.Dispatch(new TasksActions.LoadTasks());
	}

	[EffectMethod]
	public async Task HandleSetOverdueFilter(TasksActions.SetOverdueFilter action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Applying overdue filter: {IsOverdue}", action.IsOverdue);
		
		// Filter is already set in reducer, now reload tasks
		await Task.CompletedTask;
		dispatcher.Dispatch(new TasksActions.LoadTasks());
	}

	[EffectMethod]
	public async Task HandleClearFilters(TasksActions.ClearFilters action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Clearing all filters");
		
		// Filters cleared in reducer, now reload tasks
		await Task.CompletedTask;
		dispatcher.Dispatch(new TasksActions.LoadTasks());
	}
}
