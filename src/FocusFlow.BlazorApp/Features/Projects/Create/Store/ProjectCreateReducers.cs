using Fluxor;

namespace FocusFlow.BlazorApp.Features.Projects.Create.Store;

public static class ProjectCreateReducers
{
    [ReducerMethod]
    public static ProjectCreateState ReduceCreateProjectAction(ProjectCreateState state, CreateProjectAction action) =>
        state with { IsCreating = true, Error = null };

    [ReducerMethod]
    public static ProjectCreateState ReduceCreateProjectSuccessAction(ProjectCreateState state, CreateProjectSuccessAction action) =>
        state with { IsCreating = false, CreatedProject = action.Project, Error = null };

    [ReducerMethod]
    public static ProjectCreateState ReduceCreateProjectFailureAction(ProjectCreateState state, CreateProjectFailureAction action) =>
        state with { IsCreating = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectCreateState ReduceClearCreateProjectErrorAction(ProjectCreateState state, ClearCreateProjectErrorAction action) =>
        state with { Error = null };
}