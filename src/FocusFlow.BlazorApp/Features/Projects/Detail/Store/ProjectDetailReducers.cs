using Fluxor;
using System.Collections.Generic;
using System.Linq;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

/// <summary>
/// Reducers for handling project detail state changes.
/// </summary>
public static class ProjectDetailReducers
{
    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailAction(ProjectDetailState state, LoadProjectDetailAction action) =>
        state with { IsLoading = true, Error = null, ErrorMessage = null };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailSuccessAction(ProjectDetailState state, LoadProjectDetailSuccessAction action) =>
        state with { IsLoading = false, Project = action.Project, ErrorMessage = null };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailFailureAction(ProjectDetailState state, LoadProjectDetailFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    // Reducer for starting the create task process
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskAction(ProjectDetailState state, CreateTaskAction action) =>
        state with { IsCreatingTask = true, ErrorMessage = null }; // ✅ Set loading state and clear previous errors

    // Reducer for successfully creating a task
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskSuccessAction(ProjectDetailState state, CreateTaskSuccessAction action)
    {
        if (state.Project is null)
        {
            return state with { IsCreatingTask = false, ErrorMessage = "Cannot add a task to a null project." };
        }

        var newTasks = state.Project.Tasks.ToList();
        newTasks.Add(action.Task);

        var updatedProject = state.Project with { Tasks = newTasks };

        return state with { IsCreatingTask = false, Project = updatedProject, ErrorMessage = null };
    }

    // Reducer for failing to create a task
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskFailureAction(ProjectDetailState state, CreateTaskFailureAction action) =>
        state with { IsCreatingTask = false, ErrorMessage = action.ErrorMessage };

    // Reducer for clearing error messages
    [ReducerMethod]
    public static ProjectDetailState ReduceClearError(ProjectDetailState state, ClearProjectDetailErrorAction action) =>
        state with { ErrorMessage = null };

    //////// SignalR Reducers for real-time task updates
    
    /// <summary>
    /// Adds a new task to the project from SignalR notification
    /// </summary>
    [ReducerMethod]
    public static ProjectDetailState ReduceAddTaskToProjectSuccess(ProjectDetailState state, AddTaskToProjectSuccessAction action)
    {
        if (state.Project is null)
            return state;

        var newTasks = state.Project.Tasks.ToList();
        
        // Only add if not already present (prevent duplicates)
        if (!newTasks.Any(t => t.Id == action.Task.Id))
        {
            newTasks.Add(action.Task);
            var updatedProject = state.Project with { Tasks = newTasks };
            return state with { Project = updatedProject };
        }

        return state;
    }

    /// <summary>
    /// Updates an existing task in the project from SignalR notification
    /// </summary>
    [ReducerMethod]
    public static ProjectDetailState ReduceUpdateTaskInProjectSuccess(ProjectDetailState state, UpdateTaskInProjectSuccessAction action)
    {
        if (state.Project is null)
            return state;

        var tasks = state.Project.Tasks.ToList();
        var existingTaskIndex = tasks.FindIndex(t => t.Id == action.Task.Id);
        
        if (existingTaskIndex >= 0)
        {
            tasks[existingTaskIndex] = action.Task;
            var updatedProject = state.Project with { Tasks = tasks };
            return state with { Project = updatedProject };
        }

        return state;
    }

    /// <summary>
    /// Removes a task from the project from SignalR notification
    /// </summary>
    [ReducerMethod]
    public static ProjectDetailState ReduceRemoveTaskFromProjectSuccess(ProjectDetailState state, RemoveTaskFromProjectSuccessAction action)
    {
        if (state.Project is null)
            return state;

        var tasks = state.Project.Tasks.Where(t => t.Id != action.TaskId).ToList();
        var updatedProject = state.Project with { Tasks = tasks };
        return state with { Project = updatedProject };
    }
}
