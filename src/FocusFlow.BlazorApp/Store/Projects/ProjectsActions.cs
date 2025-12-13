using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.BlazorApp.Models; // For form models

namespace FocusFlow.BlazorApp.Store.Projects;

// Load Projects Actions
public record LoadProjectsAction();
public record LoadProjectsSuccessAction(List<ProjectDto> Projects);
public record LoadProjectsFailureAction(string Error);

// Create Project Actions
public record CreateProjectAction(ProjectCreateFormModel FormModel);
public record CreateProjectSuccessAction(ProjectDto Project);
public record CreateProjectFailureAction(string Error);

// Update Project Actions
public record UpdateProjectAction(Guid Id, ProjectUpdateFormModel FormModel);
public record UpdateProjectSuccessAction(ProjectDto Project); 
public record UpdateProjectFailureAction(string Error);

// Delete Project Actions
public record DeleteProjectAction(Guid Id);
public record DeleteProjectSuccessAction(Guid Id);
public record DeleteProjectFailureAction(string Error);
