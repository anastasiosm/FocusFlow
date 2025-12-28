using FocusFlow.BlazorApp.Features.Projects.Detail.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

public record LoadProjectDetailAction(Guid ProjectId);
public record LoadProjectDetailSuccessAction(ProjectDetailDto Project);
public record LoadProjectDetailFailureAction(string Error);

/// <summary>
/// Clears error message from state
/// </summary>
public record ClearProjectDetailErrorAction();
