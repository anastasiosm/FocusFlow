using Fluxor;
using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Store;

[FeatureState]
public record ProjectEditState
{
    public bool IsUpdating { get; init; }
    public string? Error { get; init; }
    public ProjectDto? UpdatedProject { get; init; }

    public ProjectEditState()
    {
        IsUpdating = false;
        Error = null;
        UpdatedProject = null;
    }
}