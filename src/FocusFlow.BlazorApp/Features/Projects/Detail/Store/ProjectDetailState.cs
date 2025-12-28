using FocusFlow.BlazorApp.Features.Projects.Detail.Models;
using Fluxor;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

[FeatureState]
public record ProjectDetailState
{
    public bool IsLoading { get; init; }
    public bool IsCreatingTask { get; init; }  // ✅ Loading state for task creation
    public string? Error { get; init; }
    public string? ErrorMessage { get; init; }  // For create task errors
    public ProjectDetailDto? Project { get; init; }

    // Private parameterless constructor for Fluxor
    private ProjectDetailState()
    {
        IsLoading = false;
        IsCreatingTask = false;
        Error = null;
        ErrorMessage = null;
        Project = null;
    }

    // Public constructor for creating new instances
    public ProjectDetailState(bool isLoading, bool isCreatingTask, string? error, string? errorMessage, ProjectDetailDto? project)
    {
        IsLoading = isLoading;
        IsCreatingTask = isCreatingTask;
        Error = error;
        ErrorMessage = errorMessage;
        Project = project;
    }
}
