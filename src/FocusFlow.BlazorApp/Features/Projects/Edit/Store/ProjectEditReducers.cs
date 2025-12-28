using Fluxor;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Store;

public static class ProjectEditReducers
{
    [ReducerMethod]
    public static ProjectEditState ReduceUpdateProjectAction(ProjectEditState state, UpdateProjectAction action) =>
        state with { IsUpdating = true, Error = null };

    [ReducerMethod]
    public static ProjectEditState ReduceUpdateProjectSuccessAction(ProjectEditState state, UpdateProjectSuccessAction action) =>
        state with { IsUpdating = false, UpdatedProject = action.Project, Error = null };

    [ReducerMethod]
    public static ProjectEditState ReduceUpdateProjectFailureAction(ProjectEditState state, UpdateProjectFailureAction action) =>
        state with { IsUpdating = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectEditState ReduceClearEditProjectErrorAction(ProjectEditState state, ClearEditProjectErrorAction action) =>
        state with { Error = null };
}