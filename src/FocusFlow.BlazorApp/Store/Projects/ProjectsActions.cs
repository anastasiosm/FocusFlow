using FocusFlow.BlazorApp.Models.Dtos;
using FocusFlow.BlazorApp.Models;

namespace FocusFlow.BlazorApp.Store.Projects;

/// <summary>
/// Actions for Projects feature - Fluxor state management
/// All actions follow the pattern: {Verb}{Noun}[Success|Failure]Action
/// </summary>

// ============================================================================
// Load Projects
// ============================================================================

/// <summary>
/// Triggers loading of all projects for the current user.
/// Effect will fetch projects from API and dispatch success/failure.
/// </summary>
public record LoadProjectsAction();

/// <summary>
/// Dispatched when projects are successfully loaded from API.
/// Updates state with the list of projects.
/// </summary>
/// <param name="Projects">List of project DTOs from API</param>
public record LoadProjectsSuccessAction(List<ProjectDto> Projects);

/// <summary>
/// Dispatched when project loading fails.
/// Updates state with error message to display to user.
/// </summary>
/// <param name="Error">Error message describing what went wrong</param>
public record LoadProjectsFailureAction(string Error);

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
/// Adds the new project to state and optionally navigates to project detail.
/// </summary>
/// <param name="Project">The newly created project DTO from API</param>
public record CreateProjectSuccessAction(ProjectDto Project);

/// <summary>
/// Dispatched when project creation fails (validation or API error).
/// Updates state with error message to display to user.
/// </summary>
/// <param name="Error">Error message describing what went wrong</param>
public record CreateProjectFailureAction(string Error);

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

// ============================================================================
// Delete Project
// ============================================================================

/// <summary>
/// Triggers deletion of a project.
/// Effect will call API and dispatch success/failure.
/// WARNING: This is a destructive action that cannot be undone.
/// </summary>
/// <param name="Id">ID of the project to delete</param>
public record DeleteProjectAction(Guid Id);

/// <summary>
/// Dispatched when project is successfully deleted.
/// Removes the project from state.
/// </summary>
/// <param name="Id">ID of the deleted project</param>
public record DeleteProjectSuccessAction(Guid Id);

/// <summary>
/// Dispatched when project deletion fails.
/// Updates state with error message to display to user.
/// </summary>
/// <param name="Error">Error message describing what went wrong</param>
public record DeleteProjectFailureAction(string Error);