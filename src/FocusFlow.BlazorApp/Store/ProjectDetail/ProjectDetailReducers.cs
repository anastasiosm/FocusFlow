using Fluxor;
using System.Collections.Generic;
using System.Linq;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public static class ProjectDetailReducers
{
    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailAction(ProjectDetailState state, LoadProjectDetailAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailSuccessAction(ProjectDetailState state, LoadProjectDetailSuccessAction action) =>
        state with { IsLoading = false, Project = action.Project };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailFailureAction(ProjectDetailState state, LoadProjectDetailFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    // Reducer for starting the create task process
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskAction(ProjectDetailState state, CreateTaskAction action) =>
        state with { IsLoading = true, Error = null }; // Or a more specific loading flag

    // Reducer for successfully creating a task
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskSuccessAction(ProjectDetailState state, CreateTaskSuccessAction action)
    {
        if (state.Project is null)
        {
            return state with { IsLoading = false, Error = "Cannot add a task to a null project." };
        }

        var newTasks = state.Project.Tasks.ToList();
        newTasks.Add(action.Task);

        var updatedProject = state.Project with { Tasks = newTasks };

        return state with { IsLoading = false, Project = updatedProject };
    }

    // Reducer for failing to create a task
    [ReducerMethod]
    public static ProjectDetailState ReduceCreateTaskFailureAction(ProjectDetailState state, CreateTaskFailureAction action) =>
        state with { IsLoading = false, Error = action.ErrorMessage };
}
