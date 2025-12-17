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

		// 2. Create Task - FIXED: Added Description
		var taskRequest = new 
		{ 
			ProjectId = project!.Id,
			Title = "My Task", 
			Description = "Do something important", // FIXED: Description is required
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

		// FIXED: Added Description to all tasks
		await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project!.Id,
			Title = "T1",
			Description = "Task 1 description",
			DueDate = DateTime.UtcNow.AddDays(7),
			Priority = 1,
			AssignedUserId = (string?)null
		});
		await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project!.Id,
			Title = "T2",
			Description = "Task 2 description",
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
		projRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
		project.Should().NotBeNull();
		
		// Create Task - FIXED: Added Description, DueDate and validation
		var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Original Title",
			Description = "Original Description", // FIXED: Required field
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2
		});
		
		// FIXED: Validate task was created successfully
		createTaskRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var createdTask = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();
		createdTask.Should().NotBeNull();

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
		
		// FIXED: Added Description and DueDate
		var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Task to Delete",
			Description = "This task will be deleted",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2
		});
		
		// FIXED: Ensure task was created successfully
		createTaskRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var task = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();
		task.Should().NotBeNull();

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
		projRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
		project.Should().NotBeNull();
		
		// FIXED: Added Description, DueDate and validation
		var createTaskRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Task",
			Description = "Task description",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2
		});
		createTaskRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var task = await createTaskRes.Content.ReadFromJsonAsync<TaskDto>();
		task.Should().NotBeNull();

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
		
		// Create multiple tasks with different statuses - FIXED: Added Description, DueDate and validation
		var task1Res = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Task 1",
			Description = "Task 1 description",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2
		});
		task1Res.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var task1 = await task1Res.Content.ReadFromJsonAsync<TaskDto>();
		task1.Should().NotBeNull();
		
		var task2Res = await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Task 2",
			Description = "Task 2 description",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2
		});
		task2Res.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var task2 = await task2Res.Content.ReadFromJsonAsync<TaskDto>();
		task2.Should().NotBeNull();
		
		// Update task2 to InProgress
		var statusUpdateRes = await _client.PatchAsJsonAsync($"/api/tasks/{task2!.Id}/status", new { Status = 2 });
		statusUpdateRes.StatusCode.Should().Be(HttpStatusCode.OK);

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
		projRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();
		project.Should().NotBeNull();
		
		// Create tasks with different priorities - FIXED: Added Description, DueDate (required) and validation
		var highPriorityRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project!.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "High Priority Task",
			Description = "High priority description",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 2 // High
		});
		highPriorityRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
		
		var lowPriorityRes = await _client.PostAsJsonAsync($"/api/tasks?projectId={project.Id}", new 
		{ 
			ProjectId = project.Id,
			Title = "Low Priority Task",
			Description = "Low priority description",
			DueDate = DateTime.UtcNow.AddDays(7), // FIXED: Required field
			Priority = 0 // Low
		});
		lowPriorityRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

		// Act - Filter by High priority
		var response = await _client.GetAsync("/api/tasks?priority=2");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var filteredTasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
		filteredTasks.Should().HaveCount(1);
		filteredTasks!.First().Priority.Should().Be(2);
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
			Description = "Trying to create task in someone else's project",
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
