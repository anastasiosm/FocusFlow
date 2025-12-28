using FocusFlow.BlazorApp.Features.Projects.Edit.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Store;

/// <summary>
/// Actions for Project Edit feature
/// </summary>

// ============================================================================
// Update Project
// ============================================================================

/// <summary>
/// Triggers update of an existing project.
/// Effect will validate form, call API, and dispatch success/failure.
/// </summary>
/// <param name="Id">ID of the project to update</param>
/// <param name="FormModel">Form model containing updated project data</param>
public record UpdateProjectAction(Guid Id, ProjectUpdateFormModel FormModel);

/// <summary>
/// Dispatched when project is successfully updated.
/// Updates the project in state with new data.
/// </summary>
/// <param name="Project">The updated project DTO from API</param>
public record UpdateProjectSuccessAction(ProjectDto Project);

/// <summary>
/// Dispatched when project update fails (validation or API error).
/// Updates state with error message to display to user.
/// </summary>
/// <param name="Error">Error message describing what went wrong</param>
public record UpdateProjectFailureAction(string Error);

/// <summary>
/// Clears any error messages from the edit project state
/// </summary>
public record ClearEditProjectErrorAction();