using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FocusFlow.Integration.Tests;

[Collection("IntegrationTests")]
public class ProjectsControllerTests : IntegrationTestBase
{
    public ProjectsControllerTests(FocusFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyUserProjects()
    {
        // Arrange
        await AuthenticateAsync("user1", "user1@example.com");
        
        // Create a project for user1
        await _client.PostAsJsonAsync("/api/projects", new { Name = "User1 Project", Description = "Desc" });

        // Authenticate as user2
        await AuthenticateAsync("user2", "user2@example.com");

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        projects.Should().BeEmpty(); // User2 should not see User1's project
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedProject()
    {
        // Arrange
        await AuthenticateAsync();

        var request = new { Name = "New Project", Description = "Description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task Update_WithOwnership_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateAsync();

        // Create project
        var createResponse = await _client.PostAsJsonAsync("/api/projects", new { Name = "My Project", Description = "Desc" });
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var updateRequest = new { Name = "Updated Name", Description = "Updated Desc" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{createdProject!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithoutOwnership_ShouldReturnUnauthorized()
    {
        // Arrange
        await AuthenticateAsync("owner", "owner@example.com");
        var createResponse = await _client.PostAsJsonAsync("/api/projects", new { Name = "Owner Project", Description = "Owner Desc" });
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Switch user
        await AuthenticateAsync("intruder", "intruder@example.com");
        
        var updateRequest = new { Name = "Hacked Name", Description = "Hacked Desc" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{createdProject!.Id}", updateRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithOwnership_ShouldReturnNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/projects", new { Name = "To Delete", Description = "Desc" });
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

// Helper DTOs
public record ProjectDto(Guid Id, string Name, string? Description, string OwnerId, DateTime CreatedAt, DateTime UpdatedAt, int TaskCount);
