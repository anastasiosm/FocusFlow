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

	// Task Detail effects
	[EffectMethod]
	public async Task HandleLoadTaskById(TasksActions.LoadTaskById action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Loading task by ID: {TaskId}", action.TaskId);

		var result = await _apiService.GetTaskByIdAsync(action.TaskId);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully loaded task: {TaskId}", action.TaskId);
			dispatcher.Dispatch(new TasksActions.LoadTaskByIdSuccess(result.Data!));
		}
		else
		{
			_logger.LogError("Failed to load task {TaskId}: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TasksActions.LoadTaskByIdFailure(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleUpdateTask(TasksActions.UpdateTask action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Updating task: {TaskId}", action.TaskId);

		var updateDto = new Models.UpdateTaskDto(action.Title, action.Description, action.DueDate, action.Priority);
		var result = await _apiService.UpdateTaskAsync(action.TaskId, updateDto);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully updated task: {TaskId}", action.TaskId);
			dispatcher.Dispatch(new TasksActions.UpdateTaskSuccess(result.Data!));
		}
		else
		{
			_logger.LogError("Failed to update task {TaskId}: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TasksActions.UpdateTaskFailure(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleUpdateTaskStatus(TasksActions.UpdateTaskStatus action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Updating task status: {TaskId} to {Status}", action.TaskId, action.Status);

		var result = await _apiService.UpdateTaskStatusAsync(action.TaskId, action.Status);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully updated task status: {TaskId}", action.TaskId);
			// Reload the task to get updated data
			dispatcher.Dispatch(new TasksActions.LoadTaskById(action.TaskId));
		}
		else
		{
			_logger.LogError("Failed to update task status {TaskId}: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TasksActions.UpdateTaskStatusFailure(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleDeleteTask(TasksActions.DeleteTask action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Deleting task: {TaskId}", action.TaskId);

		var result = await _apiService.DeleteTaskAsync(action.TaskId);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully deleted task: {TaskId}", action.TaskId);
			dispatcher.Dispatch(new TasksActions.DeleteTaskSuccess(action.TaskId));
		}
		else
		{
			_logger.LogError("Failed to delete task {TaskId}: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TasksActions.DeleteTaskFailure(result.Error ?? "Unknown error"));
		}
	}
}
