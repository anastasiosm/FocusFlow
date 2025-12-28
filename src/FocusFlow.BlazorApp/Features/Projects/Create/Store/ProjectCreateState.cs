using Fluxor;
using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Create.Store;

[FeatureState]
public record ProjectCreateState
{
    public bool IsCreating { get; init; }
    public string? Error { get; init; }
    public ProjectDto? CreatedProject { get; init; }

    public ProjectCreateState()
    {
        IsCreating = false;
        Error = null;
        CreatedProject = null;
    }
}