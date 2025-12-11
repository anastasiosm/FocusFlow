using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using System.Net.Http.Json;

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

	// Projects
	public async Task<List<ProjectDto>> GetProjectsAsync()
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<List<ProjectDto>>("api/projects");
			return result ?? new List<ProjectDto>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching projects");
			throw;
		}
	}

	public async Task<ProjectDetailDto> GetProjectByIdAsync(Guid id)
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<ProjectDetailDto>($"api/projects/{id}");
			return result ?? throw new InvalidOperationException($"Project {id} not found");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching project {ProjectId}", id);
			throw;
		}
	}

	public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("api/projects", dto);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
			return result ?? throw new InvalidOperationException("Failed to create project");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating project");
			throw;
		}
	}

	public async Task<ProjectDto> UpdateProjectAsync(Guid id, string name, string? description)
	{
		try
		{
			var dto = new { Name = name, Description = description };
			var response = await _httpClient.PutAsJsonAsync($"api/projects/{id}", dto);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
			return result ?? throw new InvalidOperationException("Failed to update project");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating project {ProjectId}", id);
			throw;
		}
	}

	public async Task DeleteProjectAsync(Guid id)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"api/projects/{id}");
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting project {ProjectId}", id);
			throw;
		}
	}

	// Tasks
	public async Task<List<TaskDto>> GetTasksAsync(Guid projectId)
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<List<TaskDto>>($"api/projects/{projectId}/tasks");
			return result ?? new List<TaskDto>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching tasks for project {ProjectId}", projectId);
			throw;
		}
	}

	public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync($"api/tasks?projectId={dto.ProjectId}", dto);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<TaskDto>();
			return result ?? throw new InvalidOperationException("Failed to create task");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating task");
			throw;
		}
	}

	public async Task DeleteTaskAsync(Guid id)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting task {TaskId}", id);
			throw;
		}
	}
}