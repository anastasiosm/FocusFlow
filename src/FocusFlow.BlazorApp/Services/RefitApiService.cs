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
	public Task<ApiResult<string>> LoginAsync(LoginRequest request) =>
	ExecuteApiCall(
		async () =>
		{
			var result = await _authApi.LoginAsync(request);
			return result.Token ?? throw new InvalidOperationException("No token received");
		},
		"Login");

	public Task<ApiResult> RegisterAsync(RegisterRequest request) =>
		ExecuteApiCall(
			() => _authApi.RegisterAsync(request),
			"Register");

	// Projects
	public Task<ApiResult<List<ProjectDto>>> GetProjectsAsync() =>
		ExecuteApiCall(
			async () => await _projectsApi.GetProjectsAsync() ?? new List<ProjectDto>(),
			"Get projects");

	public Task<ApiResult<ProjectDetailDto>> GetProjectByIdAsync(Guid id) =>
		ExecuteApiCall(
			() => _projectsApi.GetProjectByIdAsync(id),
			$"Get project {id}");

	public Task<ApiResult<ProjectDto>> CreateProjectAsync(CreateProjectDto dto) =>
		ExecuteApiCall(
			() => _projectsApi.CreateProjectAsync(dto),
			"Create project");

	public Task<ApiResult> UpdateProjectAsync(Guid id, UpdateProjectDto dto) =>
		ExecuteApiCall(
			() => _projectsApi.UpdateProjectAsync(id, dto),
			$"Update project {id}");

	public Task<ApiResult> DeleteProjectAsync(Guid id) =>
		ExecuteApiCall(
			() => _projectsApi.DeleteProjectAsync(id),
			$"Delete project {id}");

	// Tasks
	public Task<ApiResult<List<TaskResponse>>> GetTasksAsync(Guid projectId) =>
		ExecuteApiCall(
			async () => await _tasksApi.GetTasksAsync(projectId) ?? new List<TaskResponse>(),
			$"Get tasks for project {projectId}");

	public Task<ApiResult<List<TaskResponse>>> GetTasksFilteredAsync(
		ProjectTaskStatus? status = null,
		Priority? priority = null,
		bool? isOverdue = null) =>
		ExecuteApiCall(
			async () => await _tasksApi.GetTasksFilteredAsync(status, priority, isOverdue) ?? new List<TaskResponse>(),
			"Get filtered tasks");

	public Task<ApiResult<TaskResponse>> CreateTaskAsync(CreateTaskRequest dto) =>
		ExecuteApiCall(
			async () =>
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

				return await _tasksApi.CreateTaskAsync(request);
			},
			$"Create task for project {dto.ProjectId}");

	public Task<ApiResult<TaskResponse>> GetTaskByIdAsync(Guid id) =>
		ExecuteApiCall(
			() => _tasksApi.GetTaskByIdAsync(id),
			$"Get task {id}");

	public Task<ApiResult<TaskResponse>> UpdateTaskAsync(Guid id, UpdateTaskRequest dto) =>
		ExecuteApiCall(
			async () =>
			{
				var request = new UpdateTaskRequest
				{
					Title = dto.Title,
					Description = dto.Description,
					DueDate = dto.DueDate,
					Priority = dto.Priority
				};
				return await _tasksApi.UpdateTaskAsync(id, request);
			},
			$"Update task {id}");

	public Task<ApiResult> UpdateTaskStatusAsync(Guid id, ProjectTaskStatus status) =>
		ExecuteApiCall(
			async () =>
			{
				var request = new UpdateTaskStatusRequest { Status = status };
				await _tasksApi.UpdateTaskStatusAsync(id, request);
			},
			$"Update task status {id}");

	public Task<ApiResult> DeleteTaskAsync(Guid id) =>
		ExecuteApiCall(
			() => _tasksApi.DeleteTaskAsync(id),
			$"Delete task {id}");

	// Dashboard
	public Task<ApiResult<List<ProjectStatisticsDto>>> GetDashboardStatisticsAsync() =>
		ExecuteApiCall(
			async () => await _dashboardApi.GetDashboardStatisticsAsync() ?? new List<ProjectStatisticsDto>(),
			"Get dashboard statistics");

	// ===============================================
	// HELPER METHODS
	// ===============================================

	/// <summary>
	/// Executes an API call that returns data (Task&lt;T&gt;).
	/// </summary>
	/// <param name="unwrapInnerResult">If true, expects the action to return ApiResult&lt;T&gt; instead of T</param>
	private async Task<ApiResult<T>> ExecuteApiCall<T>(
		Func<Task<T>> action,
		string operationName,
		bool unwrapInnerResult = false)
	{
		try
		{
			var result = await action();

			// Handle special case where action returns ApiResult<T>
			if (unwrapInnerResult && result is ApiResult<T> innerResult)
			{
				return innerResult;
			}

			return ApiResult<T>.Success(result);
		}
		catch (ApiException apiEx)
		{
			var error = await GetErrorMessage(apiEx);
			_logger.LogError(apiEx, "API Error: {Operation}. Status: {StatusCode}", operationName, apiEx.StatusCode);
			return ApiResult<T>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error: {Operation}", operationName);
			return ApiResult<T>.Failure($"An unexpected error occurred during {operationName}.");
		}
	}

	/// <summary>
	/// Executes an API call that returns void (Task).
	/// </summary>
	private async Task<ApiResult> ExecuteApiCall(Func<Task> action, string operationName)
	{
		try
		{
			await action();
			return ApiResult.Success();
		}
		catch (ApiException apiEx)
		{
			var error = await GetErrorMessage(apiEx);
			_logger.LogError(apiEx, "API Error: {Operation}. Status: {StatusCode}", operationName, apiEx.StatusCode);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error: {Operation}", operationName);
			return ApiResult.Failure($"An unexpected error occurred during {operationName}.");
		}
	}

	/// <summary>
	/// Extracts error messages from API exceptions (ValidationErrorResponse or ProblemDetails).
	/// </summary>
	private async Task<string> GetErrorMessage(ApiException apiEx)
	{
		_logger.LogDebug("API Exception Content: {Content}", apiEx.Content ?? "No content");

		if (!apiEx.HasContent || string.IsNullOrWhiteSpace(apiEx.Content))
		{
			return $"API Error: {apiEx.StatusCode}";
		}

		try
		{
			// Try ValidationErrorResponse (ASP.NET Core validation errors)
			var validationError = await apiEx.GetContentAsAsync<ValidationErrorResponse>();

			if (validationError?.Errors != null && validationError.Errors.Any())
			{
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

			if (!string.IsNullOrEmpty(validationError?.Error))
			{
				_logger.LogDebug("Using error field: {Error}", validationError.Error);
				return validationError.Error;
			}
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Failed to parse as ValidationErrorResponse");
		}

		try
		{
			// Try ProblemDetails (RFC 7807)
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

		// Fallback: return raw content (truncated)
		var rawContent = apiEx.Content.Length > 500
			? apiEx.Content.Substring(0, 500) + "..."
			: apiEx.Content;

		_logger.LogWarning("Could not parse error response. Raw content: {Content}", rawContent);
		return $"API Error: {apiEx.StatusCode}. {rawContent}";
	}
}

// ===============================================
// RESPONSE MODELS
// ===============================================

public class ValidationErrorResponse
{
	public string? Error { get; set; }
	public Dictionary<string, string[]>? Errors { get; set; }
	public int StatusCode { get; set; }
	public string? TraceId { get; set; }
	public string? Path { get; set; }
}

public class ProblemDetails
{
	public string? Type { get; set; }
	public string? Title { get; set; }
	public int? Status { get; set; }
	public string? Detail { get; set; }
	public string? Instance { get; set; }
	public Dictionary<string, object>? Extensions { get; set; }
}