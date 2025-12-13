using FocusFlow.Application.Features.Projects.GetProjectById;
using Fluxor;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

[FeatureState]
public record ProjectDetailState
{
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
    public ProjectDetailDto? Project { get; init; }

    // Private parameterless constructor for Fluxor
    private ProjectDetailState()
    {
        IsLoading = false;
        Error = null;
        Project = null;
    }

    // Public constructor for creating new instances
    public ProjectDetailState(bool isLoading, string? error, ProjectDetailDto? project)
    {
        IsLoading = isLoading;
        Error = error;
        Project = project;
    }
}
