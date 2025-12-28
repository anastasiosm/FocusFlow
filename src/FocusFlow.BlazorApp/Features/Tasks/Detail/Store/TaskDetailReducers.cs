using Fluxor;

namespace FocusFlow.BlazorApp.Features.Tasks.Detail.Store;

/// <summary>
/// Reducers for Task Detail state
/// </summary>
public static class TaskDetailReducers
{
	[ReducerMethod]
	public static TaskDetailState OnLoadTaskById(TaskDetailState state, TaskDetailActions.LoadTaskByIdAction action)
	{
		return state with
		{
			IsLoading = true,
			ErrorMessage = null
		};
	}
	[ReducerMethod]
	public static TaskDetailState OnLoadTaskByIdSuccess(TaskDetailState state, TaskDetailActions.LoadTaskByIdSuccessAction action)
	{
		return state with
		{
			Task = action.Task,
			IsLoading = false,
			ErrorMessage = null
		};
	}
	[ReducerMethod]
	public static TaskDetailState OnLoadTaskByIdFailure(TaskDetailState state, TaskDetailActions.LoadTaskByIdFailureAction action)
	{
		return state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task Status reducers
	[ReducerMethod]
	public static TaskDetailState OnUpdateTaskStatus(TaskDetailState state, TaskDetailActions.UpdateTaskStatusAction action)
	{
		return state with
		{
			IsUpdatingStatus = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TaskDetailState OnUpdateTaskStatusSuccess(TaskDetailState state, TaskDetailActions.UpdateTaskStatusSuccessAction action)
	{
		return state with
		{
			IsUpdatingStatus = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TaskDetailState OnUpdateTaskStatusFailure(TaskDetailState state, TaskDetailActions.UpdateTaskStatusFailureAction action)
	{
		return state with
		{
			IsUpdatingStatus = false,
			ErrorMessage = action.ErrorMessage
		};
	}
}
