using Fluxor;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Reducers for Tasks state
/// </summary>
public static class TasksReducers
{
	[ReducerMethod]
	public static TasksState OnLoadTasks(TasksState state, TasksActions.LoadTasksAction action)
	{
		return state with
		{
			IsLoading = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTasksSuccess(TasksState state, TasksActions.LoadTasksSuccessAction action)
	{
		return state with
		{
			Tasks = action.Tasks,
			IsLoading = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTasksFailure(TasksState state, TasksActions.LoadTasksFailureAction action)
	{
		return state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	[ReducerMethod]
	public static TasksState OnSetStatusFilter(TasksState state, TasksActions.SetStatusFilterAction action)
	{
		return state with
		{
			StatusFilter = action.Status,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnSetPriorityFilter(TasksState state, TasksActions.SetPriorityFilterAction action)
	{
		return state with
		{
			PriorityFilter = action.Priority,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnSetOverdueFilter(TasksState state, TasksActions.SetOverdueFilterAction action)
	{
		return state with
		{
			IsOverdueFilter = action.IsOverdue,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksState OnClearFilters(TasksState state, TasksActions.ClearFiltersAction action)
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
	public static TasksState OnLoadTaskById(TasksState state, TasksActions.LoadTaskByIdAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTaskByIdSuccess(TasksState state, TasksActions.LoadTaskByIdSuccessAction action)
	{
		return state with
		{
			SelectedTask = action.Task,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnLoadTaskByIdFailure(TasksState state, TasksActions.LoadTaskByIdFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task reducers
	[ReducerMethod]
	public static TasksState OnUpdateTask(TasksState state, TasksActions.UpdateTaskFromResultAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskSuccess(TasksState state, TasksActions.UpdateTaskSuccessAction action)
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
	public static TasksState OnUpdateTaskFailure(TasksState state, TasksActions.UpdateTaskFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task Status reducers
	[ReducerMethod]
	public static TasksState OnUpdateTaskStatus(TasksState state, TasksActions.UpdateTaskStatusAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnUpdateTaskStatusSuccess(TasksState state, TasksActions.UpdateTaskStatusSuccessAction action)
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
	public static TasksState OnUpdateTaskStatusFailure(TasksState state, TasksActions.UpdateTaskStatusFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Delete Task reducers
	[ReducerMethod]
	public static TasksState OnDeleteTask(TasksState state, TasksActions.DeleteTaskAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksState OnDeleteTaskSuccess(TasksState state, TasksActions.DeleteTaskSuccessAction action)
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
	public static TasksState OnDeleteTaskFailure(TasksState state, TasksActions.DeleteTaskFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}
}
