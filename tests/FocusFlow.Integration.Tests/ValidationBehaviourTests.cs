using FluentAssertions;
using FocusFlow.Application;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FocusFlow.Integration.Tests;

public class ValidationBehaviourTests
{
	[Fact]
	public async Task Send_WithInvalidCommand_ShouldThrowValidationException()
	{
		// Arrange
		var services = new ServiceCollection();

		// Register Application Services (including MediatR, validators, and the ValidationBehaviour)
		services.AddApplicationServices();

		// Mock dependencies that handlers might need
		var projectRepositoryMock = new Mock<IProjectRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		services.AddSingleton(projectRepositoryMock.Object);
		services.AddSingleton(unitOfWorkMock.Object);

		var serviceProvider = services.BuildServiceProvider();

		// Get the Mediator instance
		var mediator = serviceProvider.GetRequiredService<IMediator>();

		// Create an invalid command
		var invalidCommand = new CreateProjectCommand("", "Some Description", "user123");

		// Act & Assert
		// This will fail if the ValidationBehaviour is not registered or not working correctly
		var exception = await Assert.ThrowsAsync<FocusFlowValidationException>(() =>
			mediator.Send(invalidCommand));

		// Assert that the exception contains the expected error
		exception.Errors.Should().NotBeEmpty();
		exception.Errors.Should().ContainKey("Name");
		exception.Errors["Name"].Should().Contain("Project name is required");
	}
}
