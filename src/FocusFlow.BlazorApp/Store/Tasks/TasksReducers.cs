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

	// Task Detail reducers
	[ReducerMethod]
	public static TasksState OnLoadTaskById(TasksState state, TasksActions.LoadTaskById action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTaskByIdSuccess(TasksState state, TasksActions.LoadTaskByIdSuccess action)
	{
		return state with
		{
			SelectedTask = action.Task,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTaskByIdFailure(TasksState state, TasksActions.LoadTaskByIdFailure action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task reducers
	[ReducerMethod]
	public static TasksState OnUpdateTask(TasksState state, TasksActions.UpdateTask action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskSuccess(TasksState state, TasksActions.UpdateTaskSuccess action)
	{
		var updatedTasks = state.Tasks.Select(t => t.Id == action.Task.Id ? action.Task : t).ToList();
		
		return state with
		{
			Tasks = updatedTasks,
			SelectedTask = action.Task,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskFailure(TasksState state, TasksActions.UpdateTaskFailure action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task Status reducers
	[ReducerMethod]
	public static TasksState OnUpdateTaskStatus(TasksState state, TasksActions.UpdateTaskStatus action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskStatusSuccess(TasksState state, TasksActions.UpdateTaskStatusSuccess action)
	{
		var updatedTasks = state.Tasks.Select(t => t.Id == action.Task.Id ? action.Task : t).ToList();
		
		return state with
		{
			Tasks = updatedTasks,
			SelectedTask = action.Task,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskStatusFailure(TasksState state, TasksActions.UpdateTaskStatusFailure action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Delete Task reducers
	[ReducerMethod]
	public static TasksState OnDeleteTask(TasksState state, TasksActions.DeleteTask action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnDeleteTaskSuccess(TasksState state, TasksActions.DeleteTaskSuccess action)
	{
		var updatedTasks = state.Tasks.Where(t => t.Id != action.TaskId).ToList();
		
		return state with
		{
			Tasks = updatedTasks,
			SelectedTask = state.SelectedTask?.Id == action.TaskId ? null : state.SelectedTask,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnDeleteTaskFailure(TasksState state, TasksActions.DeleteTaskFailure action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}
}
