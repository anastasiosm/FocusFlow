using Fluxor;

namespace FocusFlow.BlazorApp.Features.Tasks.List.Store;

/// <summary>
/// Reducers for Tasks state
/// </summary>
public static class TasksListReducers
{
	[ReducerMethod]
	public static TasksListState OnLoadTasks(TasksListState state, TasksListActions.LoadTasksAction action)
	{
		return state with
		{
			IsLoading = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnLoadTasksSuccess(TasksListState state, TasksListActions.LoadTasksSuccessAction action)
	{
		return state with
		{
			Tasks = action.Tasks,
			IsLoading = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnLoadTasksFailure(TasksListState state, TasksListActions.LoadTasksFailureAction action)
	{
		return state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	[ReducerMethod]
	public static TasksListState OnSetStatusFilter(TasksListState state, TasksListActions.SetStatusFilterAction action)
	{
		return state with
		{
			StatusFilter = action.Status,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksListState OnSetPriorityFilter(TasksListState state, TasksListActions.SetPriorityFilterAction action)
	{
		return state with
		{
			PriorityFilter = action.Priority,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksListState OnSetOverdueFilter(TasksListState state, TasksListActions.SetOverdueFilterAction action)
	{
		return state with
		{
			IsOverdueFilter = action.IsOverdue,
			IsLoading = false
		};
	}

	[ReducerMethod]
	public static TasksListState OnClearFilters(TasksListState state, TasksListActions.ClearFiltersAction action)
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
	public static TasksListState OnLoadTaskById(TasksListState state, TasksListActions.LoadTaskByIdAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnLoadTaskByIdSuccess(TasksListState state, TasksListActions.LoadTaskByIdSuccessAction action)
	{
		return state with
		{
			SelectedTask = action.Task,
			IsLoadingTask = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnLoadTaskByIdFailure(TasksListState state, TasksListActions.LoadTaskByIdFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task reducers
	[ReducerMethod]
	public static TasksListState OnUpdateTask(TasksListState state, TasksListActions.UpdateTaskFromResultAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnUpdateTaskSuccess(TasksListState state, TasksListActions.UpdateTaskSuccessAction action)
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
	public static TasksListState OnUpdateTaskFailure(TasksListState state, TasksListActions.UpdateTaskFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Update Task Status reducers
	[ReducerMethod]
	public static TasksListState OnUpdateTaskStatus(TasksListState state, TasksListActions.UpdateTaskStatusAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnUpdateTaskStatusSuccess(TasksListState state, TasksListActions.UpdateTaskStatusSuccessAction action)
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
	public static TasksListState OnUpdateTaskStatusFailure(TasksListState state, TasksListActions.UpdateTaskStatusFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}

	// Delete Task reducers
	[ReducerMethod]
	public static TasksListState OnDeleteTask(TasksListState state, TasksListActions.DeleteTaskAction action)
	{
		return state with
		{
			IsLoadingTask = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static TasksListState OnDeleteTaskSuccess(TasksListState state, TasksListActions.DeleteTaskSuccessAction action)
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
	public static TasksListState OnDeleteTaskFailure(TasksListState state, TasksListActions.DeleteTaskFailureAction action)
	{
		return state with
		{
			IsLoadingTask = false,
			ErrorMessage = action.ErrorMessage
		};
	}
}
