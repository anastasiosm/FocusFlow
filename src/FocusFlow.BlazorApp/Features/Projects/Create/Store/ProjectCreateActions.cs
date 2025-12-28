using FocusFlow.BlazorApp.Features.Projects.Create.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Create.Store;

/// <summary>
/// Actions for Project Creation feature
/// </summary>

// ============================================================================
// Create Project
// ============================================================================

/// <summary>
/// Triggers creation of a new project.
/// Effect will validate form, call API, and dispatch success/failure.
/// </summary>
/// <param name="FormModel">Form model containing project name and description</param>
public record CreateProjectAction(ProjectCreateFormModel FormModel);

/// <summary>
/// Dispatched when project is successfully created.
/// Contains the new project DTO from API.
/// </summary>
/// <param name="Project">The newly created project DTO from API</param>
public record CreateProjectSuccessAction(ProjectDto Project);

/// <summary>
/// Dispatched when project creation fails (validation or API error).
/// Updates state with error message to display to user.
/// </summary>
/// <param name="Error">Error message describing what went wrong</param>
public record CreateProjectFailureAction(string Error);

/// <summary>
/// Clears any error messages from the create project state
/// </summary>
public record ClearCreateProjectErrorAction();