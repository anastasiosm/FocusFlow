using Fluxor;
using FocusFlow.Application.Features.Projects.Common;

namespace FocusFlow.BlazorApp.Store.Projects;

public static class ProjectsReducers
{
    [ReducerMethod]
    public static ProjectsState ReduceLoadProjectsAction(ProjectsState state, LoadProjectsAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceLoadProjectsSuccessAction(ProjectsState state, LoadProjectsSuccessAction action) =>
        state with { IsLoading = false, Projects = action.Projects, Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceLoadProjectsFailureAction(ProjectsState state, LoadProjectsFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectsState ReduceCreateProjectAction(ProjectsState state, CreateProjectAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceCreateProjectSuccessAction(ProjectsState state, CreateProjectSuccessAction action) =>
        state with { IsLoading = false, Projects = state.Projects.Append(action.Project).ToList(), Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceCreateProjectFailureAction(ProjectsState state, CreateProjectFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectsState ReduceUpdateProjectAction(ProjectsState state, UpdateProjectAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceUpdateProjectSuccessAction(ProjectsState state, UpdateProjectSuccessAction action)
    {
        var projectToUpdate = state.Projects.FirstOrDefault(p => p.Id == action.Project.Id);
        if (projectToUpdate != null)
        {
            var updatedProjects = state.Projects.Select(p => p.Id == action.Project.Id ? action.Project : p).ToList();
            return state with { IsLoading = false, Projects = updatedProjects, Error = null };
        }
        return state with { IsLoading = false, Error = null };
    }

    [ReducerMethod]
    public static ProjectsState ReduceUpdateProjectFailureAction(ProjectsState state, UpdateProjectFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectsState ReduceDeleteProjectAction(ProjectsState state, DeleteProjectAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceDeleteProjectSuccessAction(ProjectsState state, DeleteProjectSuccessAction action) =>
        state with { IsLoading = false, Projects = state.Projects.Where(p => p.Id != action.Id).ToList(), Error = null };

    [ReducerMethod]
    public static ProjectsState ReduceDeleteProjectFailureAction(ProjectsState state, DeleteProjectFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };
}
