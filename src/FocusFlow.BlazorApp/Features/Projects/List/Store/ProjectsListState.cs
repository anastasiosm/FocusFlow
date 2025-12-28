using Fluxor;
using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.List.Store;

[FeatureState]
public record ProjectsListState
{
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
    public List<ProjectDto> Projects { get; init; }

    public ProjectsListState()
    {
        IsLoading = false;
        Error = null;
        Projects = new List<ProjectDto>();
    }

    public ProjectsListState(bool isLoading, string? error, List<ProjectDto> projects)
    {
        IsLoading = isLoading;
        Error = error;
        Projects = projects;
    }
}