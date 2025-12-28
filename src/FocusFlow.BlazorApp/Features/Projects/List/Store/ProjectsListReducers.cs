using Fluxor;

namespace FocusFlow.BlazorApp.Features.Projects.List.Store;

public static class ProjectsListReducers
{
    [ReducerMethod]
    public static ProjectsListState ReduceLoadProjectsAction(ProjectsListState state, LoadProjectsAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsListState ReduceLoadProjectsSuccessAction(ProjectsListState state, LoadProjectsSuccessAction action) =>
        state with { IsLoading = false, Projects = action.Projects, Error = null };

    [ReducerMethod]
    public static ProjectsListState ReduceLoadProjectsFailureAction(ProjectsListState state, LoadProjectsFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectsListState ReduceAddProjectToListAction(ProjectsListState state, AddProjectToListAction action) =>
        state with { Projects = [..state.Projects, action.Project] };

    [ReducerMethod]
    public static ProjectsListState ReduceDeleteProjectAction(ProjectsListState state, DeleteProjectAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsListState ReduceDeleteProjectSuccessAction(ProjectsListState state, DeleteProjectSuccessAction action) =>
        state with { IsLoading = false, Projects = state.Projects.Where(p => p.Id != action.Id).ToList(), Error = null };

    [ReducerMethod]
    public static ProjectsListState ReduceDeleteProjectFailureAction(ProjectsListState state, DeleteProjectFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };
}