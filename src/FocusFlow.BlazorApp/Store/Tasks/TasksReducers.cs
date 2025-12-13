using Fluxor;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Reducers for Tasks state
/// </summary>
public static class TasksReducers
{
	[ReducerMethod]
	public static TasksState OnLoadTasks(TasksState state, TasksActions.LoadTasks action)
	{
		return state with
		{
			IsLoading = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTasksSuccess(TasksState state, TasksActions.LoadTasksSuccess action)
	{
		return state with
		{
			Tasks = action.Tasks,
			IsLoading = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTasksFailure(TasksState state, TasksActions.LoadTasksFailure action)
	{
		return state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	[ReducerMethod]
	public static TasksState OnSetStatusFilter(TasksState state, TasksActions.SetStatusFilter action)
	{
		return state with
		{
			StatusFilter = action.Status,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnSetPriorityFilter(TasksState state, TasksActions.SetPriorityFilter action)
	{
		return state with
		{
			PriorityFilter = action.Priority,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnSetOverdueFilter(TasksState state, TasksActions.SetOverdueFilter action)
	{
		return state with
		{
			IsOverdueFilter = action.IsOverdue,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnClearFilters(TasksState state, TasksActions.ClearFilters action)
	{
		return state with
		{
			StatusFilter = null,
			PriorityFilter = null,
			IsOverdueFilter = null,
			IsLoading = false
		};
	}
}
