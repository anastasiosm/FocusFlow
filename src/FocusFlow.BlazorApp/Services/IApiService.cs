using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;

namespace FocusFlow.BlazorApp.Services;

public interface IApiService
{
	// Projects
	Task<List<ProjectDto>> GetProjectsAsync();
	Task<ProjectDetailDto> GetProjectByIdAsync(Guid id);
	Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
	Task<ProjectDto> UpdateProjectAsync(Guid id, string name, string? description);
	Task DeleteProjectAsync(Guid id);

	// Tasks
	Task<List<TaskDto>> GetTasksAsync(Guid projectId);
	Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
	Task DeleteTaskAsync(Guid id);
}