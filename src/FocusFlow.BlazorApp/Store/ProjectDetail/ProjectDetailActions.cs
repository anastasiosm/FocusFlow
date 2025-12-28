using FocusFlow.BlazorApp.Models.Dtos;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public record LoadProjectDetailAction(Guid ProjectId);
public record LoadProjectDetailSuccessAction(ProjectDetailDto Project);
public record LoadProjectDetailFailureAction(string Error);

/// <summary>
/// Clears error message from state
/// </summary>
public record ClearProjectDetailErrorAction();
