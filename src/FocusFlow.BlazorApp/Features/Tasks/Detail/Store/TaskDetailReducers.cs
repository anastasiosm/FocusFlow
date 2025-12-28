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
}
