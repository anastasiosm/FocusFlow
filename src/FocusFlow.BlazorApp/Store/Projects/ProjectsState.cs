using Fluxor;
using FocusFlow.Application.Features.Projects.Common;

namespace FocusFlow.BlazorApp.Store.Projects;

public record ProjectsState
{
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
    public List<ProjectDto> Projects { get; init; }

    public ProjectsState()
    {
        IsLoading = false;
        Error = null;
        Projects = new List<ProjectDto>();
    }

    public ProjectsState(bool isLoading, string? error, List<ProjectDto> projects)
    {
        IsLoading = isLoading;
        Error = error;
        Projects = projects;
    }
}

public class ProjectsFeature : Feature<ProjectsState>
{
    public override string GetName() => "Projects";
    protected override ProjectsState GetInitialState() => new ProjectsState();
}
