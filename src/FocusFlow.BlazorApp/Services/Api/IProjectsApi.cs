using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.UpdateProject;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface IProjectsApi
{
    [Get("/api/projects")]
    Task<List<ProjectDto>> GetProjectsAsync();

    [Get("/api/projects/{id}")]
    Task<ProjectDetailDto> GetProjectByIdAsync(Guid id);

    [Post("/api/projects")]
    Task<ProjectDto> CreateProjectAsync([Body] CreateProjectDto dto);

    [Put("/api/projects/{id}")]
    Task UpdateProjectAsync(Guid id, [Body] UpdateProjectDto dto);

    [Delete("/api/projects/{id}")]
    Task DeleteProjectAsync(Guid id);
}