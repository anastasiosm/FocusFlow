using FocusFlow.BlazorApp.Features.Projects.List.Models;

namespace FocusFlow.BlazorApp.Features.Projects.List.Store;

/// <summary>
/// Actions for Projects List feature - Loading and displaying projects
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
// Add Project (when created from other features)
// ============================================================================

/// <summary>
/// Adds a newly created project to the projects list.
/// Used when a project is created from other features.
/// </summary>
/// <param name="Project">The newly created project to add to the list</param>
public record AddProjectToListAction(ProjectDto Project);

// ============================================================================
// Delete Project (από το List view)
// ============================================================================

/// <summary>
/// Triggers deletion of a project from the list.
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