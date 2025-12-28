using Fluxor;
using FocusFlow.BlazorApp.Services;

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
}
