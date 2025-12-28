using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Features.Tasks.Edit.Extensions;

namespace FocusFlow.BlazorApp.Features.Tasks.Detail.Store;

/// <summary>
/// Effects for Task Detail feature
/// </summary>
public class TaskDetailEffects
{
	private readonly IApiService _apiService;
	private readonly ILogger<TaskDetailEffects> _logger;
	public TaskDetailEffects(IApiService apiService, ILogger<TaskDetailEffects> logger)
	{
		_apiService = apiService;
		_logger = logger;
	}
	[EffectMethod]
	public async Task HandleLoadTaskById(TaskDetailActions.LoadTaskByIdAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Loading task with ID: {TaskId}", action.TaskId);
		var result = await _apiService.GetTaskByIdAsync(action.TaskId);
		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully loaded task with ID: {TaskId}", action.TaskId);
			dispatcher.Dispatch(new TaskDetailActions.LoadTaskByIdSuccessAction(result.Data!));
		}
		else
		{
			_logger.LogError("Failed to load task with ID: {TaskId}, Error: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TaskDetailActions.LoadTaskByIdFailureAction(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleUpdateTaskStatus(TaskDetailActions.UpdateTaskStatusAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Updating task status: {TaskId} to {Status}", action.TaskId, action.Status);

		var result = await _apiService.UpdateTaskStatusAsync(action.TaskId, action.Status);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully updated task status: {TaskId}", action.TaskId);
			// Reload the task to get the updated data
			dispatcher.Dispatch(new TaskDetailActions.LoadTaskByIdAction(action.TaskId));
			// Dispatch success action for UI feedback
			dispatcher.Dispatch(new TaskDetailActions.UpdateTaskStatusSuccessAction());
		}
		else
		{
			_logger.LogError("Failed to update task status: {TaskId}, Error: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TaskDetailActions.UpdateTaskStatusFailureAction(result.Error ?? "Unknown error"));
		}
	}

	[EffectMethod]
	public async Task HandleUpdateTask(TaskDetailActions.UpdateTaskAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Updating task: {TaskId}", action.TaskId);

		// Convert TaskEditResult to UpdateTaskRequest using extension method
		var request = action.EditResult.ToUpdateRequest();
		var result = await _apiService.UpdateTaskAsync(action.TaskId, request);

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully updated task: {TaskId}", action.TaskId);
			// Reload the task to get the updated data
			dispatcher.Dispatch(new TaskDetailActions.LoadTaskByIdAction(action.TaskId));
			// Dispatch success action for UI feedback
			dispatcher.Dispatch(new TaskDetailActions.UpdateTaskSuccessAction());
		}
		else
		{
			_logger.LogError("Failed to update task: {TaskId}, Error: {Error}", action.TaskId, result.Error);
			dispatcher.Dispatch(new TaskDetailActions.UpdateTaskFailureAction(result.Error ?? "Unknown error"));
		}
	}
}
