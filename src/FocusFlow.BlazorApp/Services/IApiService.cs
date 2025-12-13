using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.BlazorApp.Models;

namespace FocusFlow.BlazorApp.Services;

public interface IApiService
{
	// Auth
    Task<ApiResult<string>> LoginAsync(LoginRequest request);
    Task<ApiResult> RegisterAsync(RegisterRequest request);

	// Projects
	Task<ApiResult<List<ProjectDto>>> GetProjectsAsync();
	Task<ApiResult<ProjectDetailDto>> GetProjectByIdAsync(Guid id);
	Task<ApiResult<ProjectDto>> CreateProjectAsync(CreateProjectDto dto);
    Task<ApiResult> DeleteProjectAsync(Guid id);
	Task<ApiResult> UpdateProjectAsync(Guid id, UpdateProjectDto dto);
	// Tasks
	Task<ApiResult<List<TaskDto>>> GetTasksAsync(Guid projectId);
	Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskDto dto);
	Task<ApiResult> DeleteTaskAsync(Guid id);
}