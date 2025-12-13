using System.Net.Http;
using System.Net;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using System.Net.Http.Json;
using FocusFlow.BlazorApp.Models; 
using System.Text.Json;

namespace FocusFlow.BlazorApp.Services;

public class ApiService : IApiService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ApiService> _logger;

	public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	    // Auth
    public async Task<ApiResult<string>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(); 
            
            return ApiResult<string>.Success(result?.Token ?? throw new InvalidOperationException("Login failed: No token received"));
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error logging in");
            string error = await GetErrorMessage(httpEx);
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
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            response.EnsureSuccessStatusCode();
            return ApiResult.Success();
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error registering");
            string error = await GetErrorMessage(httpEx);
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
		// Diagnostic comment to force re-evaluation
		try
		{
			var result = await _httpClient.GetFromJsonAsync<List<ProjectDto>>("api/projects");
			return ApiResult<List<ProjectDto>>.Success(result ?? new List<ProjectDto>());
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error fetching projects");
            string error = await GetErrorMessage(httpEx);
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
			var result = await _httpClient.GetFromJsonAsync<ProjectDetailDto>($"api/projects/{id}");
			return ApiResult<ProjectDetailDto>.Success(result ?? throw new InvalidOperationException($"Project {id} not found"));
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error fetching project {ProjectId}", id);
            string error = await GetErrorMessage(httpEx);
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
			var response = await _httpClient.PostAsJsonAsync("api/projects", dto);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
			return ApiResult<ProjectDto>.Success(result ?? throw new InvalidOperationException("Failed to create project"));
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error creating project");
            string error = await GetErrorMessage(httpEx);
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
			var response = await _httpClient.PutAsJsonAsync($"api/projects/{id}", dto);
			response.EnsureSuccessStatusCode();
            return ApiResult.Success();
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error updating project {ProjectId}", id);
            string error = await GetErrorMessage(httpEx);
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
			var response = await _httpClient.DeleteAsync($"api/projects/{id}");
			response.EnsureSuccessStatusCode();
            return ApiResult.Success();
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error deleting project {ProjectId}", id);
            string error = await GetErrorMessage(httpEx);
            return ApiResult.Failure(error);
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting project {ProjectId}", id);
			return ApiResult.Failure($"An unexpected error occurred while deleting project {id}.");
		}
	}

	// Tasks
	public async Task<ApiResult<List<TaskDto>>> GetTasksAsync(Guid projectId)
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<List<TaskDto>>($"api/projects/{projectId}/tasks");
			return ApiResult<List<TaskDto>>.Success(result ?? new List<TaskDto>());
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error fetching tasks for project {ProjectId}", projectId);
            string error = await GetErrorMessage(httpEx);
            return ApiResult<List<TaskDto>>.Failure(error);
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching tasks for project {ProjectId}", projectId);
			return ApiResult<List<TaskDto>>.Failure($"An unexpected error occurred while fetching tasks for project {projectId}.");
		}
	}

	public async Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskDto dto)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync($"api/tasks?projectId={dto.ProjectId}", dto);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<TaskDto>();
			return ApiResult<TaskDto>.Success(result ?? throw new InvalidOperationException("Failed to create task"));
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error creating task");
            string error = await GetErrorMessage(httpEx);
            return ApiResult<TaskDto>.Failure(error);
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating task");
			return ApiResult<TaskDto>.Failure("An unexpected error occurred during task creation.");
		}
	}

	public async Task<ApiResult> DeleteTaskAsync(Guid id)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
			response.EnsureSuccessStatusCode();
            return ApiResult.Success();
		}
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Error deleting task {TaskId}", id);
            string error = await GetErrorMessage(httpEx);
            return ApiResult.Failure(error);
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting task {TaskId}", id);
			return ApiResult.Failure($"An unexpected error occurred while deleting task {id}.");
		}
	}

    private async Task<string> GetErrorMessage(HttpRequestException httpEx)
    {
        if (httpEx.StatusCode.HasValue)
        {
            return $"An HTTP error occurred: {(int)httpEx.StatusCode.Value} {httpEx.StatusCode.Value}";
        }
        return "An unexpected network error occurred.";
    }
}

// ProblemDetails class for deserialization
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
}