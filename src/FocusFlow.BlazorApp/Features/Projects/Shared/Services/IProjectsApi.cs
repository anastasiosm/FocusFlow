using FocusFlow.BlazorApp.Features.Projects.List.Models;
using FocusFlow.BlazorApp.Features.Projects.Detail.Models;
using FocusFlow.BlazorApp.Features.Projects.Create.Models;
using FocusFlow.BlazorApp.Features.Projects.Edit.Models;
using Refit;

namespace FocusFlow.BlazorApp.Features.Projects.Shared.Services;

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