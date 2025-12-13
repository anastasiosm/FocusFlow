using Fluxor;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public static class ProjectDetailReducers
{
    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailAction(ProjectDetailState state, LoadProjectDetailAction action) =>
        state with { IsLoading = true, Error = null, Project = null };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailSuccessAction(ProjectDetailState state, LoadProjectDetailSuccessAction action) =>
        state with { IsLoading = false, Project = action.Project };

    [ReducerMethod]
    public static ProjectDetailState ReduceLoadProjectDetailFailureAction(ProjectDetailState state, LoadProjectDetailFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };
}
