using FluentAssertions;
using FocusFlow.Application.Features.Dashboard.Common;
using FocusFlow.Application.Features.Projects.Common;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FocusFlow.Integration.Tests;

[Collection("IntegrationTests")]
public class DashboardControllerTests : IntegrationTestBase
{
	public DashboardControllerTests(FocusFlowWebApplicationFactory factory) : base(factory)
	{
	}

	[Fact]
	public async Task GetStatistics_WithAuthentication_ShouldReturnStatistics()
	{
		// Arrange
		await AuthenticateAsync("testuser", "testuser@example.com");

		// Create some projects
		var project1Response = await _client.PostAsJsonAsync("/api/projects", 
			new { Name = "Project 1", Description = "Test project 1" });
		var project1 = await project1Response.Content.ReadFromJsonAsync<ProjectDto>();

		var project2Response = await _client.PostAsJsonAsync("/api/projects", 
			new { Name = "Project 2", Description = "Test project 2" });
		var project2 = await project2Response.Content.ReadFromJsonAsync<ProjectDto>();

		// Act
		var response = await _client.GetAsync("/api/dashboard/statistics");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var statistics = await response.Content.ReadFromJsonAsync<List<ProjectStatisticsDto>>();
		
		statistics.Should().NotBeNull();
		statistics.Should().HaveCount(2);
		statistics.Should().Contain(s => s.ProjectName == "Project 1");
		statistics.Should().Contain(s => s.ProjectName == "Project 2");
	}

	[Fact]
	public async Task GetStatistics_WithoutAuthentication_ShouldReturnUnauthorized()
	{
		// Act
		var response = await _client.GetAsync("/api/dashboard/statistics");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task GetStatistics_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange
		await AuthenticateAsync("newuser", "newuser@example.com");

		// Act
		var response = await _client.GetAsync("/api/dashboard/statistics");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var statistics = await response.Content.ReadFromJsonAsync<List<ProjectStatisticsDto>>();
		
		statistics.Should().NotBeNull();
		statistics.Should().BeEmpty();
	}

	[Fact]
	public async Task GetStatistics_ShouldOnlyReturnUserOwnedProjects()
	{
		// Arrange - User 1 creates projects
		await AuthenticateAsync("user1", "user1@example.com");
		await _client.PostAsJsonAsync("/api/projects", 
			new { Name = "User1 Project", Description = "User1's project" });

		// User 2 creates projects
		await AuthenticateAsync("user2", "user2@example.com");
		var user2ProjectResponse = await _client.PostAsJsonAsync("/api/projects", 
			new { Name = "User2 Project", Description = "User2's project" });
		var user2Project = await user2ProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();

		// Act - Get statistics for User2
		var response = await _client.GetAsync("/api/dashboard/statistics");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var statistics = await response.Content.ReadFromJsonAsync<List<ProjectStatisticsDto>>();
		
		statistics.Should().NotBeNull();
		statistics.Should().HaveCount(1);
		statistics!.First().ProjectName.Should().Be("User2 Project");
	}
}
