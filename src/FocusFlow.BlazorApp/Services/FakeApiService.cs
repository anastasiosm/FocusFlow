using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Application.Features.Dashboard.Common;
using FocusFlow.BlazorApp.Models;
using FocusFlow.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace FocusFlow.BlazorApp.Services;

public class FakeApiService : IApiService
{
    private readonly ILogger<FakeApiService> _logger;
    private readonly List<ProjectDetailDto> _projects = new();
    private readonly List<TaskDto> _tasks = new();
    private readonly Dictionary<string, (string Username, string Password)> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    public FakeApiService(ILogger<FakeApiService> logger)
    {
        _logger = logger;

        // Seed a default user
        _usersByEmail["test@example.com"] = ("test", "password");

        // Seed initial data
        var project1Id = Guid.NewGuid();
        var project2Id = Guid.NewGuid();

        _projects.Add(new ProjectDetailDto(
            project1Id,
            "Blazor UI Refactor",
            "Complete the refactoring of the Blazor UI to use Fluxor state management and improve UX.",
            "user-fake-id",
            DateTime.UtcNow.AddDays(-10), // CreatedAt
            DateTime.UtcNow.AddDays(-1),  // UpdatedAt
            new List<TaskDto>()
        ));

        _projects.Add(new ProjectDetailDto(
            project2Id,
            "API Endpoint Security",
            "Implement JWT authentication and authorization for all API endpoints.",
            "user-fake-id",
            DateTime.UtcNow.AddDays(-20), // CreatedAt
            DateTime.UtcNow.AddDays(-5),  // UpdatedAt
            new List<TaskDto>()
        ));

        _tasks.Add(new TaskDto(Guid.NewGuid(), "Implement Skeleton Loaders", "High priority UI enhancement.", DateTime.UtcNow.AddDays(2), ProjectTaskStatus.InProgress, Priority.High, null, project1Id, "user-fake-id", DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1)));
        _tasks.Add(new TaskDto(Guid.NewGuid(), "Add Confirmation Dialogs", "Implement confirmation prompts for destructive actions.", DateTime.UtcNow.AddDays(-1), ProjectTaskStatus.Done, Priority.Medium, DateTime.UtcNow.AddDays(-1), project1Id, "user-fake-id", DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1)));
        _tasks.Add(new TaskDto(Guid.NewGuid(), "Configure JWT Middleware", "Set up JWT token validation and authentication in the backend.", DateTime.UtcNow.AddDays(-6), ProjectTaskStatus.Done, Priority.High, DateTime.UtcNow.AddDays(-6), project2Id, "user-fake-id", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-6)));
    }

    // Auth
    public Task<ApiResult<string>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("FakeApiService: LoginAsync called. Email='{Email}', PasswordLength={PasswordLength}",
            request.Email,
            request.Password?.Length ?? 0);

        if (_usersByEmail.TryGetValue(request.Email, out var user) && user.Password == request.Password)
        {
            _logger.LogInformation("FakeApiService: LoginAsync success");
            var token = CreateFakeJwt(user.Username, request.Email);
            return Task.FromResult(ApiResult<string>.Success(token));
        }

        _logger.LogWarning("FakeApiService: LoginAsync failure (invalid credentials)");
        return Task.FromResult(ApiResult<string>.Failure("Invalid credentials."));
    }

    public Task<ApiResult> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("FakeApiService: RegisterAsync called. Email='{Email}', Username='{Username}', PasswordLength={PasswordLength}",
            request.Email,
            request.Username,
            request.Password?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(ApiResult.Failure("Missing registration fields."));
        }

        if (_usersByEmail.ContainsKey(request.Email))
        {
            return Task.FromResult(ApiResult.Failure("Email already registered."));
        }

        _usersByEmail[request.Email] = (request.Username, request.Password);
        _logger.LogInformation("FakeApiService: RegisterAsync stored user. TotalUsers={TotalUsers}", _usersByEmail.Count);

        return Task.FromResult(ApiResult.Success());
    }

    private static string CreateFakeJwt(string username, string email)
    {
        // NOTE: This is NOT a real JWT signature. It's just shaped like a JWT so JwtParser can read it.
        var headerJson = JsonSerializer.Serialize(new { alg = "none", typ = "JWT" });

        var payload = new Dictionary<string, object?>
        {
            [ClaimTypes.Name] = username,
            ["email"] = email,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var payloadJson = JsonSerializer.Serialize(payload);

        static string ToBase64NoPadding(string s)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes).TrimEnd('=');
        }

        var header = ToBase64NoPadding(headerJson);
        var body = ToBase64NoPadding(payloadJson);

        return $"{header}.{body}.sig";
    }

    // Projects
    public Task<ApiResult<List<ProjectDto>>> GetProjectsAsync()
    {
        var projectDtos = _projects.Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.OwnerId, p.CreatedAt, p.UpdatedAt, _tasks.Count(t => t.ProjectId == p.Id))).ToList();
        return Task.FromResult(ApiResult<List<ProjectDto>>.Success(projectDtos));
    }

    public Task<ApiResult<ProjectDetailDto>> GetProjectByIdAsync(Guid id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project is null)
        {
            return Task.FromResult(ApiResult<ProjectDetailDto>.Failure("Project not found."));
        }

        // Get tasks for the project
        var projectTasks = _tasks.Where(t => t.ProjectId == id).ToList();
        var projectWithTasks = project with { Tasks = projectTasks };

        return Task.FromResult(ApiResult<ProjectDetailDto>.Success(projectWithTasks));
    }

    public Task<ApiResult<ProjectDto>> CreateProjectAsync(CreateProjectDto dto)
    {
        var newProject = new ProjectDetailDto(
            Guid.NewGuid(),
            dto.Name,
            dto.Description,
            "user-fake-id",
            DateTime.UtcNow, // CreatedAt
            DateTime.UtcNow, // UpdatedAt
            new List<TaskDto>()
        );
        _projects.Add(newProject);
        
        var projectDto = new ProjectDto(newProject.Id, newProject.Name, newProject.Description, newProject.OwnerId, newProject.CreatedAt, newProject.UpdatedAt, 0);
        return Task.FromResult(ApiResult<ProjectDto>.Success(projectDto));
    }

    public Task<ApiResult> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project is null)
        {
            return Task.FromResult(ApiResult.Failure("Project not found."));
        }

        var updatedProject = project with { Name = dto.Name, Description = dto.Description, UpdatedAt = DateTime.UtcNow };
        _projects.Remove(project);
        _projects.Add(updatedProject);

        return Task.FromResult(ApiResult.Success());
    }

    public Task<ApiResult> DeleteProjectAsync(Guid id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project != null)
        {
            _projects.Remove(project);
            return Task.FromResult(ApiResult.Success());
        }
        return Task.FromResult(ApiResult.Failure("Project not found."));
    }

    // Tasks
    public Task<ApiResult<List<TaskDto>>> GetTasksAsync(Guid projectId)
    {
        var tasks = _tasks.Where(t => t.ProjectId == projectId).ToList();
        return Task.FromResult(ApiResult<List<TaskDto>>.Success(tasks));
    }

    public Task<ApiResult<List<TaskDto>>> GetTasksFilteredAsync(ProjectTaskStatus? status = null, Priority? priority = null, bool? isOverdue = null)
    {
        try
        {
            var filteredTasks = _tasks.AsEnumerable();

            if (status.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == priority.Value);
            }

            if (isOverdue.HasValue && isOverdue.Value)
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.DueDate.HasValue && 
                    t.DueDate.Value < DateTime.UtcNow && 
                    t.Status != ProjectTaskStatus.Done);
            }

            var result = filteredTasks.ToList();
            _logger.LogInformation("FakeApiService: Retrieved {Count} filtered tasks", result.Count);
            return Task.FromResult(ApiResult<List<TaskDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FakeApiService.GetTasksFilteredAsync");
            return Task.FromResult(ApiResult<List<TaskDto>>.Failure("Failed to retrieve filtered tasks."));
        }
    }

    public Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskDto dto)
    {
        var projectId = dto.ProjectId;
        var newTask = new TaskDto(
            Guid.NewGuid(),
            dto.Title,
            dto.Description,
            dto.DueDate,
            ProjectTaskStatus.Todo,
            dto.Priority,
            null, 
            projectId,
            dto.AssignedUserId,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _tasks.Add(newTask);
        
        // Also add it to the project's task list for consistency
        var project = _projects.FirstOrDefault(p => p.Id == projectId);
        if (project != null)
        {
            var updatedTasks = project.Tasks.ToList();
            updatedTasks.Add(newTask);
            var updatedProject = project with { Tasks = updatedTasks };
            _projects.Remove(project);
            _projects.Add(updatedProject);
        }

        return Task.FromResult(ApiResult<TaskDto>.Success(newTask));
    }

    public Task<ApiResult> DeleteTaskAsync(Guid id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task != null)
        {
            _tasks.Remove(task);
            return Task.FromResult(ApiResult.Success());
        }
        return Task.FromResult(ApiResult.Failure("Task not found."));
    }

    // Dashboard
    public Task<ApiResult<List<ProjectStatisticsDto>>> GetDashboardStatisticsAsync()
    {
        try
        {
            var statistics = _projects.Select(project =>
            {
                var projectTasks = _tasks.Where(t => t.ProjectId == project.Id).ToList();
                var totalTasks = projectTasks.Count;
                var completedTasks = projectTasks.Count(t => t.Status == ProjectTaskStatus.Done);
                var overdueTasks = projectTasks.Count(t => 
                    t.DueDate.HasValue && 
                    t.DueDate.Value < DateTime.UtcNow && 
                    t.Status != ProjectTaskStatus.Done);

                return new ProjectStatisticsDto(
                    project.Id,
                    project.Name,
                    totalTasks,
                    completedTasks,
                    overdueTasks);
            }).ToList();

            _logger.LogInformation("FakeApiService: Retrieved {Count} project statistics", statistics.Count);
            return Task.FromResult(ApiResult<List<ProjectStatisticsDto>>.Success(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FakeApiService.GetDashboardStatisticsAsync");
            return Task.FromResult(ApiResult<List<ProjectStatisticsDto>>.Failure("Failed to retrieve dashboard statistics."));
        }
    }
}
