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
}

// Helper DTO
public record TaskDto(Guid Id, string Title, string? Description, int Priority, int Status);
