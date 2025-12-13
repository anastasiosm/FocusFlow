using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FocusFlow.Integration.Tests;

[Collection("IntegrationTests")]
public class TasksControllerTests : IntegrationTestBase
{
    public TasksControllerTests(FocusFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateAsync();
        
        // 1. Create Project
        var projRes = await _client.PostAsJsonAsync("/api/projects", new 
        { 
            Name = "Task Project",
            Description = (string?)null 
        });
        
        // Ensure project was created successfully
        projRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();

        // 2. Create Task
        var taskRequest = new 
        { 
            ProjectId = project!.Id,
            Title = "My Task", 
            Description = "Do something",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 1, // High
            AssignedUserId = (string?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", taskRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Title.Should().Be("My Task");
    }

    [Fact]
    public async Task GetTasks_ByProject_ShouldReturnList()
    {
        // Arrange
        await AuthenticateAsync();
        var projRes = await _client.PostAsJsonAsync("/api/projects", new 
        { 
            Name = "Task Project",
            Description = (string?)null 
        });
        
        // Ensure project was created successfully
        projRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();

        await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project!.Id,
            Title = "T1",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 1,
            AssignedUserId = (string?)null
        });
        await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project!.Id,
            Title = "T2",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 1,
            AssignedUserId = (string?)null
        });

        // Act - Get tasks filtered for current user (no projectId filter needed)
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Create Project
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        // Create Task
        var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Original Title",
            Description = "Original Desc",
            Priority = 2
        });
        var createdTask = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();

        var updateRequest = new
        {
            Title = "Updated Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.AddDays(10),
            Priority = 1
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        updatedTask!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Task to Delete",
            Priority = 2
        });
        var task = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{task!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Task",
            Priority = 2
        });
        var task = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();

        var statusUpdate = new { Status = 2 }; // InProgress = 2

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{task!.Id}/status", statusUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        updatedTask!.Status.Should().Be(2);
    }

    [Fact]
    public async Task GetTasksFiltered_ByStatus_ShouldReturnFilteredList()
    {
        // Arrange
        await AuthenticateAsync();
        
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        // Create multiple tasks with different statuses
        var task1Res = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Task 1",
            Priority = 2
        });
        var task1 = await task1Res.Content.ReadFromJsonAsync<TaskDto>();
        
        var task2Res = await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Task 2",
            Priority = 2
        });
        var task2 = await task2Res.Content.ReadFromJsonAsync<TaskDto>();
        
        // Update task2 to InProgress
        await _client.PatchAsJsonAsync($"/api/tasks/{task2!.Id}/status", new { Status = 2 });

        // Act - Filter by InProgress status
        var response = await _client.GetAsync("/api/tasks?status=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var filteredTasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        filteredTasks.Should().HaveCount(1);
        filteredTasks!.First().Status.Should().Be(2);
    }

    [Fact]
    public async Task GetTasksFiltered_ByPriority_ShouldReturnFilteredList()
    {
        // Arrange
        await AuthenticateAsync();
        
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        // Create tasks with different priorities
        await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "High Priority Task",
            Priority = 2 // High
        });
        
        await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Low Priority Task",
            Priority = 0 // Low
        });

        // Act - Filter by High priority
        var response = await _client.GetAsync("/api/tasks?priority=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var filteredTasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        filteredTasks.Should().HaveCount(1);
        filteredTasks!.First().Priority.Should().Be(2);
    }

    [Fact]
    public async Task GetTasksFiltered_ByOverdue_ShouldReturnOverdueTasks()
    {
        // Arrange
        await AuthenticateAsync();
        
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
        
        // Create overdue task (due date in the past)
        await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Overdue Task",
            DueDate = DateTime.UtcNow.AddDays(-5), // 5 days ago
            Priority = 2
        });
        
        // Create future task
        await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", new 
        { 
            ProjectId = project.Id,
            Title = "Future Task",
            DueDate = DateTime.UtcNow.AddDays(5),
            Priority = 2
        });

        // Act - Filter by overdue
        var response = await _client.GetAsync("/api/tasks?isOverdue=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var filteredTasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        filteredTasks.Should().HaveCount(1);
        filteredTasks!.First().Title.Should().Be("Overdue Task");
    }

    [Fact]
    public async Task CreateTask_InOtherUsersProject_ShouldReturnForbidden()
    {
        // Arrange - User 1 creates project
        await AuthenticateAsync("user1", "user1@example.com");
        var projRes = await _client.PostAsJsonAsync("/api/projects", new { Name = "User1 Project", Description = (string?)null });
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();

        // User 2 tries to create task in User1's project
        await AuthenticateAsync("user2", "user2@example.com");

        var taskRequest = new 
        { 
            ProjectId = project!.Id,
            Title = "Unauthorized Task",
            Priority = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", taskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

// Helper DTO
public record TaskDto(Guid Id, string Title, string? Description, int Priority, int Status);
