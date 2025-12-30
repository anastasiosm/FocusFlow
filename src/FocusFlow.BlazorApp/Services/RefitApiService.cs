using FocusFlow.BlazorApp.Shared.Models;
using FocusFlow.BlazorApp.Models;
using FocusFlow.BlazorApp.Features.Auth.Login.Models;
using FocusFlow.BlazorApp.Features.Auth.Register.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Models;
using FocusFlow.BlazorApp.Features.Projects.Detail.Models;
using FocusFlow.BlazorApp.Features.Projects.Create.Models;
using FocusFlow.BlazorApp.Features.Projects.Edit.Models;
using FocusFlow.BlazorApp.Features.Projects.Shared.Models;
using FocusFlow.BlazorApp.Features.Projects.Shared.Services;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;
using FocusFlow.BlazorApp.Services.Api;
using FocusFlow.Domain.Enums;
using Refit;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Services;
using FocusFlow.BlazorApp.Features.Tasks.Create.Models;
using FocusFlow.BlazorApp.Features.Tasks.UpdateStatus.Models;
using FocusFlow.BlazorApp.Features.Dashboard.Shared.Services;

namespace FocusFlow.BlazorApp.Services;

public class RefitApiService : IApiService
{
	private readonly IAuthApi _authApi;
	private readonly IProjectsApi _projectsApi;
	private readonly ITasksApi _tasksApi;
	private readonly IDashboardApi _dashboardApi;
	private readonly ILogger<RefitApiService> _logger;

	public RefitApiService(
		IAuthApi authApi,
		IProjectsApi projectsApi,
		ITasksApi tasksApi,
		IDashboardApi dashboardApi,
		ILogger<RefitApiService> logger)
	{
		_authApi = authApi;
		_projectsApi = projectsApi;
		_tasksApi = tasksApi;
		_dashboardApi = dashboardApi;
		_logger = logger;
	}

	// Auth
	public async Task<ApiResult<string>> LoginAsync(LoginRequest request)
	{
		try
		{
			var result = await _authApi.LoginAsync(request);
			return ApiResult<string>.Success(result.Token ?? throw new InvalidOperationException("Login failed: No token received"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error logging in. Status: {StatusCode}", apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<string>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging in");
			return ApiResult<string>.Failure("An unexpected error occurred during login.");
		}
	}

	public async Task<ApiResult> RegisterAsync(RegisterRequest request)
	{
		try
		{
			await _authApi.RegisterAsync(request);
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			// Log the raw content for debugging
			var rawContent = apiEx.Content ?? "No content";
			_logger.LogError(apiEx, "API Error registering. Status: {StatusCode}, Content: {Content}",
				apiEx.StatusCode, rawContent);

			var error = await GetErrorMessage(apiEx);
			_logger.LogWarning("Parsed error message: {ErrorMessage}", error);

			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error registering");
			return ApiResult.Failure("An unexpected error occurred during registration.");
		}
	}

	// Projects
	public async Task<ApiResult<List<ProjectDto>>> GetProjectsAsync()
	{
		try
		{
			var result = await _projectsApi.GetProjectsAsync();
			return ApiResult<List<ProjectDto>>.Success(result ?? new List<ProjectDto>());
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching projects. Status: {StatusCode}", apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<List<ProjectDto>>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching projects");
			return ApiResult<List<ProjectDto>>.Failure("An unexpected error occurred while fetching projects.");
		}
	}

	public async Task<ApiResult<ProjectDetailDto>> GetProjectByIdAsync(Guid id)
	{
		try
		{
			var result = await _projectsApi.GetProjectByIdAsync(id);
			return ApiResult<ProjectDetailDto>.Success(result ?? throw new InvalidOperationException($"Project {id} not found"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching project {ProjectId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<ProjectDetailDto>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching project {ProjectId}", id);
			return ApiResult<ProjectDetailDto>.Failure($"An unexpected error occurred while fetching project {id}.");
		}
	}

	public async Task<ApiResult<ProjectDto>> CreateProjectAsync(CreateProjectDto dto)
	{
		try
		{
			var result = await _projectsApi.CreateProjectAsync(dto);
			return ApiResult<ProjectDto>.Success(result ?? throw new InvalidOperationException("Failed to create project"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error creating project. Status: {StatusCode}", apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<ProjectDto>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating project");
			return ApiResult<ProjectDto>.Failure("An unexpected error occurred during project creation.");
		}
	}

	public async Task<ApiResult> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
	{
		try
		{
			await _projectsApi.UpdateProjectAsync(id, dto);
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error updating project {ProjectId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating project {ProjectId}", id);
			return ApiResult.Failure($"An unexpected error occurred while updating project {id}.");
		}
	}

	public async Task<ApiResult> DeleteProjectAsync(Guid id)
	{
		try
		{
			await _projectsApi.DeleteProjectAsync(id);
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error deleting project {ProjectId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting project {ProjectId}", id);
			return ApiResult.Failure($"An unexpected error occurred while deleting project {id}.");
		}
	}

	// Tasks
	public async Task<ApiResult<List<TaskResponse>>> GetTasksAsync(Guid projectId)
	{
		try
		{
			var result = await _tasksApi.GetTasksAsync(projectId);
			return ApiResult<List<TaskResponse>>.Success(result ?? new List<TaskResponse>());
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching tasks for project {ProjectId}. Status: {StatusCode}", projectId, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<List<TaskResponse>>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching tasks for project {ProjectId}", projectId);
			return ApiResult<List<TaskResponse>>.Failure($"An unexpected error occurred while fetching tasks for project {projectId}.");
		}
	}

	public async Task<ApiResult<List<TaskResponse>>> GetTasksFilteredAsync(ProjectTaskStatus? status = null, Priority? priority = null, bool? isOverdue = null)
	{
		try
		{
			var result = await _tasksApi.GetTasksFilteredAsync(status, priority, isOverdue);
			return ApiResult<List<TaskResponse>>.Success(result ?? new List<TaskResponse>());
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching filtered tasks. Status: {StatusCode}", apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<List<TaskResponse>>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching filtered tasks");
			return ApiResult<List<TaskResponse>>.Failure("An unexpected error occurred while fetching filtered tasks.");
		}
	}

	public async Task<ApiResult<TaskResponse>> CreateTaskAsync(CreateTaskRequest dto)
	{
		try
		{
			var request = new CreateTaskRequest
			{
				ProjectId = dto.ProjectId,
				Title = dto.Title,
				Description = dto.Description,
				DueDate = dto.DueDate,
				Priority = dto.Priority,
				AssignedUserId = dto.AssignedUserId
			};

			var result = await _tasksApi.CreateTaskAsync(request);
			return ApiResult<TaskResponse>.Success(result ?? throw new InvalidOperationException("Failed to create task"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error creating task for project {ProjectId}. Status: {StatusCode}", dto.ProjectId, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<TaskResponse>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating task for project {ProjectId}", dto.ProjectId);
			return ApiResult<TaskResponse>.Failure("An unexpected error occurred during task creation.");
		}
	}

	public async Task<ApiResult<TaskResponse>> GetTaskByIdAsync(Guid id)
	{
		try
		{
			var result = await _tasksApi.GetTaskByIdAsync(id);
			return ApiResult<TaskResponse>.Success(result ?? throw new InvalidOperationException($"Task {id} not found"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching task {TaskId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<TaskResponse>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching task {TaskId}", id);
			return ApiResult<TaskResponse>.Failure($"An unexpected error occurred while fetching task {id}.");
		}
	}

	public async Task<ApiResult<TaskResponse>> UpdateTaskAsync(Guid id, UpdateTaskRequest dto)
	{
		try
		{
			var request = new UpdateTaskRequest
			{
				Title = dto.Title,
				Description = dto.Description,
				DueDate = dto.DueDate,
				Priority = dto.Priority
			};
			var result = await _tasksApi.UpdateTaskAsync(id, request);
			return ApiResult<TaskResponse>.Success(result ?? throw new InvalidOperationException($"Failed to update task {id}"));
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error updating task {TaskId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<TaskResponse>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating task {TaskId}", id);
			return ApiResult<TaskResponse>.Failure($"An unexpected error occurred while updating task {id}.");
		}
	}

	public async Task<ApiResult> UpdateTaskStatusAsync(Guid id, ProjectTaskStatus status)
	{
		try
		{
			var request = new UpdateTaskStatusRequest { Status = status };
			await _tasksApi.UpdateTaskStatusAsync(id, request);
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error updating task status {TaskId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating task status {TaskId}", id);
			return ApiResult.Failure($"An unexpected error occurred while updating task status {id}.");
		}
	}

	public async Task<ApiResult> DeleteTaskAsync(Guid id)
	{
		try
		{
			await _tasksApi.DeleteTaskAsync(id);
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error deleting task {TaskId}. Status: {StatusCode}", id, apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting task {TaskId}", id);
			return ApiResult.Failure($"An unexpected error occurred while deleting task {id}.");
		}
	}

	// Dashboard
	public async Task<ApiResult<List<ProjectStatisticsDto>>> GetDashboardStatisticsAsync()
	{
		try
		{
			var result = await _dashboardApi.GetDashboardStatisticsAsync();
			return ApiResult<List<ProjectStatisticsDto>>.Success(result ?? new List<ProjectStatisticsDto>());
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "API Error fetching dashboard statistics. Status: {StatusCode}", apiEx.StatusCode);
			var error = await GetErrorMessage(apiEx);
			return ApiResult<List<ProjectStatisticsDto>>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching dashboard statistics");
			return ApiResult<List<ProjectStatisticsDto>>.Failure("An unexpected error occurred while fetching dashboard statistics.");
		}
	}

	private async Task<string> GetErrorMessage(ApiException apiEx)
	{
		// Log raw content first for debugging
		_logger.LogDebug("API Exception Content: {Content}", apiEx.Content ?? "No content");

		if (!apiEx.HasContent || string.IsNullOrWhiteSpace(apiEx.Content))
		{
			return $"API Error: {apiEx.StatusCode}";
		}

		try
		{
			// Try to parse as ValidationErrorResponse (ASP.NET Core validation errors)
			var validationError = await apiEx.GetContentAsAsync<ValidationErrorResponse>();

			if (validationError?.Errors != null && validationError.Errors.Any())
			{
				// Combine all validation errors into a readable message
				var messages = validationError.Errors
					.SelectMany(kvp => kvp.Value.Select(msg => $"{kvp.Key}: {msg}"))
					.ToList();

				if (messages.Any())
				{
					var combinedMessage = string.Join("; ", messages);
					_logger.LogDebug("Parsed validation errors: {Errors}", combinedMessage);
					return combinedMessage;
				}
			}

			// Return the general error message if available
			if (!string.IsNullOrEmpty(validationError?.Error))
			{
				_logger.LogDebug("Using error field: {Error}", validationError.Error);
				return validationError.Error;
			}
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Failed to parse as ValidationErrorResponse, trying ProblemDetails");
		}

		try
		{
			// Try to parse as ProblemDetails (RFC 7807)
			var problemDetails = await apiEx.GetContentAsAsync<ProblemDetails>();

			if (problemDetails != null)
			{
				var message = problemDetails.Detail ?? problemDetails.Title ?? $"API Error: {apiEx.StatusCode}";
				_logger.LogDebug("Parsed ProblemDetails: {Message}", message);
				return message;
			}
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Failed to parse as ProblemDetails");
		}

		// If all parsing fails, return the raw content (truncated if too long)
		var rawContent = apiEx.Content.Length > 500
			? apiEx.Content.Substring(0, 500) + "..."
			: apiEx.Content;

		_logger.LogWarning("Could not parse error response. Raw content: {Content}", rawContent);
		return $"API Error: {apiEx.StatusCode}. {rawContent}";
	}
}

// Validation error response class for ASP.NET Core validation errors
public class ValidationErrorResponse
{
	public string? Error { get; set; }
	public Dictionary<string, string[]>? Errors { get; set; }
	public int StatusCode { get; set; }
	public string? TraceId { get; set; }
	public string? Path { get; set; }
}

// ProblemDetails class for RFC 7807 format
public class ProblemDetails
{
	public string? Type { get; set; }
	public string? Title { get; set; }
	public int? Status { get; set; }
	public string? Detail { get; set; }
	public string? Instance { get; set; }
	public Dictionary<string, object>? Extensions { get; set; }
}