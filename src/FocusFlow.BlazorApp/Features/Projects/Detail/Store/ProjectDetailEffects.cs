using Fluxor;
using FocusFlow.BlazorApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

public class ProjectDetailEffects
{
	private readonly IApiService _apiService;
	private readonly AuthenticationStateProvider _authStateProvider;
	private readonly ILogger<ProjectDetailEffects> _logger;

	public ProjectDetailEffects(
		IApiService apiService,
		AuthenticationStateProvider authStateProvider,
		ILogger<ProjectDetailEffects> logger)
	{
		_apiService = apiService;
		_authStateProvider = authStateProvider;
		_logger = logger;
	}

	[EffectMethod]
	public async Task HandleLoadProjectDetailAction(
		LoadProjectDetailAction action,
		IDispatcher dispatcher)
	{
		var authState = await _authStateProvider.GetAuthenticationStateAsync();

		if (!authState.User.Identity?.IsAuthenticated ?? true)
		{
			// ⚠️ Fallback: If called too early, dispatch failure instead of silent return
			dispatcher.Dispatch(new LoadProjectDetailFailureAction("User not authenticated"));
			return;
		}

		var result = await _apiService.GetProjectByIdAsync(action.ProjectId);

		if (result.Succeeded)
			dispatcher.Dispatch(new LoadProjectDetailSuccessAction(result.Data!));
		else
			dispatcher.Dispatch(new LoadProjectDetailFailureAction(result.Error!));
	}

	/////// SignalR Effects for real-time task updates

	/// <summary>
	/// Handles SignalR notification that a task was created in this project.
	/// Fetches the new task data and adds it to the project.
	/// </summary>
	[EffectMethod]
	public async Task HandleTaskCreatedInProjectFromSignalR(
		TaskCreatedInProjectFromSignalRAction action,
		IDispatcher dispatcher)
	{
		try
		{
			_logger.LogInformation("🔄 Fetching new task data for project | TaskId: {TaskId}, ProjectId: {ProjectId}", 
				action.TaskId, action.ProjectId);

			var result = await _apiService.GetTaskByIdAsync(action.TaskId);

			if (result.Succeeded && result.Data != null)
			{
				_logger.LogInformation("✅ Added new task to project from SignalR | TaskId: {TaskId}", action.TaskId);
				dispatcher.Dispatch(new AddTaskToProjectSuccessAction(result.Data));
			}
			else
			{
				_logger.LogWarning("❌ Failed to fetch task data from SignalR | TaskId: {TaskId}, Error: {Error}", 
					action.TaskId, result.Error);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error handling task created SignalR event | TaskId: {TaskId}", action.TaskId);
		}
	}

	/// <summary>
	/// Handles SignalR notification that a task was updated in this project.
	/// Fetches the updated task data and updates it in the project.
	/// </summary>
	[EffectMethod]
	public async Task HandleTaskUpdatedInProjectFromSignalR(
		TaskUpdatedInProjectFromSignalRAction action,
		IDispatcher dispatcher)
	{
		try
		{
			_logger.LogInformation("🔄 Fetching updated task data for project | TaskId: {TaskId}, ProjectId: {ProjectId}", 
				action.TaskId, action.ProjectId);

			var result = await _apiService.GetTaskByIdAsync(action.TaskId);

			if (result.Succeeded && result.Data != null)
			{
				_logger.LogInformation("✅ Updated task in project from SignalR | TaskId: {TaskId}", action.TaskId);
				dispatcher.Dispatch(new UpdateTaskInProjectSuccessAction(result.Data));
			}
			else
			{
				_logger.LogWarning("❌ Failed to fetch updated task data from SignalR | TaskId: {TaskId}, Error: {Error}", 
					action.TaskId, result.Error);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error handling task updated SignalR event | TaskId: {TaskId}", action.TaskId);
		}
	}

	/// <summary>
	/// Handles SignalR notification that a task status changed in this project.
	/// Fetches the updated task data and updates it in the project.
	/// </summary>
	[EffectMethod]
	public async Task HandleTaskStatusChangedInProjectFromSignalR(
		TaskStatusChangedInProjectFromSignalRAction action,
		IDispatcher dispatcher)
	{
		try
		{
			_logger.LogInformation("🔄 Fetching task with status change for project | TaskId: {TaskId}, ProjectId: {ProjectId}, NewStatus: {NewStatus}", 
				action.TaskId, action.ProjectId, action.NewStatus);

			var result = await _apiService.GetTaskByIdAsync(action.TaskId);

			if (result.Succeeded && result.Data != null)
			{
				_logger.LogInformation("✅ Updated task status in project from SignalR | TaskId: {TaskId}, Status: {Status}", 
					action.TaskId, action.NewStatus);
				dispatcher.Dispatch(new UpdateTaskInProjectSuccessAction(result.Data));
			}
			else
			{
				_logger.LogWarning("❌ Failed to fetch task with status change from SignalR | TaskId: {TaskId}, Error: {Error}", 
					action.TaskId, result.Error);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error handling task status changed SignalR event | TaskId: {TaskId}", action.TaskId);
		}
	}

	/// <summary>
	/// Handles SignalR notification that a task was deleted from this project.
	/// Removes the task from the project state.
	/// </summary>
	[EffectMethod]
	public async Task HandleTaskDeletedInProjectFromSignalR(
		TaskDeletedInProjectFromSignalRAction action,
		IDispatcher dispatcher)
	{
		try
		{
			_logger.LogInformation("🗑️ Removing deleted task from project | TaskId: {TaskId}, ProjectId: {ProjectId}", 
				action.TaskId, action.ProjectId);

			dispatcher.Dispatch(new RemoveTaskFromProjectSuccessAction(action.TaskId));
			
			_logger.LogInformation("✅ Removed task from project from SignalR | TaskId: {TaskId}", action.TaskId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error handling task deleted SignalR event | TaskId: {TaskId}", action.TaskId);
		}
	}
}
