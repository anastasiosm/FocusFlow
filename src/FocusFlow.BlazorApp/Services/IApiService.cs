using FocusFlow.BlazorApp.Shared.Models;
using FocusFlow.BlazorApp.Models;
using FocusFlow.BlazorApp.Models.Dtos;
using FocusFlow.BlazorApp.Features.Auth.Login.Models;
using FocusFlow.BlazorApp.Features.Auth.Register.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Models;
using FocusFlow.BlazorApp.Features.Projects.Detail.Models;
using FocusFlow.BlazorApp.Features.Projects.Create.Models;
using FocusFlow.BlazorApp.Features.Projects.Edit.Models;
using FocusFlow.BlazorApp.Features.Projects.Shared.Models;
using FocusFlow.Domain.Enums;

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
	Task<ApiResult<List<TaskDto>>> GetTasksFilteredAsync(ProjectTaskStatus? status = null, Priority? priority = null, bool? isOverdue = null);
	Task<ApiResult<TaskDto>> GetTaskByIdAsync(Guid id);
	Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskDto dto);
	Task<ApiResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskDto dto);
	Task<ApiResult> UpdateTaskStatusAsync(Guid id, ProjectTaskStatus status);
	Task<ApiResult> DeleteTaskAsync(Guid id);
	
	// Dashboard
	Task<ApiResult<List<ProjectStatisticsDto>>> GetDashboardStatisticsAsync();
}