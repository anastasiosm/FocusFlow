using FluentAssertions;
using FocusFlow.Application.Features.Projects.GetAllProjects;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.GetProjectsByOwner;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Projects.Queries;

public class ProjectQueryTests : TestBase
{
	[Fact]
	public async Task GetAllProjectsQuery_ShouldReturnAllProjects()
	{
		// Arrange
		var projects = new List<Project>
		{
			new("Project 1", "Desc 1", "user1"),
			new("Project 2", "Desc 2", "user2"),
			new("Project 3", null, "user1")
		};

		MockProjectRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(projects);

		var handler = new GetAllProjectsQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetAllProjectsQuery();

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(p => p.Name == "Project 1");
		result.Should().Contain(p => p.Name == "Project 2");
		result.Should().Contain(p => p.Name == "Project 3");
	}

	[Fact]
	public async Task GetAllProjectsQuery_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange
		MockProjectRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project>());

		var handler = new GetAllProjectsQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetAllProjectsQuery();

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetProjectByIdQuery_WithExistingProject_ShouldReturnProject()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var project = new Project("Test Project", "Description", "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		// Add a task
		var task = new ProjectTask("Task 1", null, projectId);
		project.AddTask(task);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(project);

		var handler = new GetProjectByIdQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetProjectByIdQuery(projectId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(projectId);
		result.Name.Should().Be("Test Project");
		result.Tasks.Should().HaveCount(1);
		result.Tasks.First().Title.Should().Be("Task 1");
	}

	[Fact]
	public async Task GetProjectByIdQuery_WithNonExistingProject_ShouldThrowNotFoundException()
	{
		// Arrange
		var projectId = Guid.NewGuid();

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Project?)null);

		var handler = new GetProjectByIdQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetProjectByIdQuery(projectId);

		// Act
		Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowNotFoundException>()
			.WithMessage("*Project*not found*");
	}

	[Fact]
	public async Task GetProjectsByOwnerQuery_ShouldReturnOnlyOwnerProjects()
	{
		// Arrange
		var ownerId = "user123";
		var projects = new List<Project>
		{
			new("My Project 1", null, ownerId),
			new("My Project 2", null, ownerId)
		};

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(ownerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(projects);

		var handler = new GetProjectsByOwnerQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetProjectsByOwnerQuery(ownerId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().AllSatisfy(p => p.OwnerId.Should().Be(ownerId));
	}

	[Fact]
	public async Task GetProjectsByOwnerQuery_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange
		var ownerId = "user-no-projects";

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project>());

		var handler = new GetProjectsByOwnerQueryHandler(MockProjectRepository.Object, Mapper);
		var query = new GetProjectsByOwnerQuery(ownerId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}