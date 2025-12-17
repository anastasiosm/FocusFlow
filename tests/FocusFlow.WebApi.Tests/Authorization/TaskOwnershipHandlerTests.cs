using FluentAssertions;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.GetTaskById;
using FocusFlow.Domain.Enums;
using FocusFlow.WebApi.Authorization.TaskOwnership;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FocusFlow.WebApi.Tests.Authorization;

public class TaskOwnershipHandlerTests
{
	private readonly Mock<IMediator> _mockMediator;
	private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
	private readonly Mock<ILogger<TaskOwnershipHandler>> _mockLogger;
	private readonly TaskOwnershipHandler _handler;

	public TaskOwnershipHandlerTests()
	{
		_mockMediator = new Mock<IMediator>();
		_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
		_mockLogger = new Mock<ILogger<TaskOwnershipHandler>>();

		_handler = new TaskOwnershipHandler(
			_mockMediator.Object,
			_mockHttpContextAccessor.Object,
			_mockLogger.Object);
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenUserOwnsProjectContainingTask_ShouldSucceed()
	{
		// Arrange
		var userId = "user123";
		var projectId = Guid.NewGuid();
		var taskId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = taskId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		var taskDto = new TaskDto(
			taskId,
			"Test Task",
			null,
			null,
			ProjectTaskStatus.Todo,
			Priority.Medium,
			null,
			projectId,
			null,
			DateTime.UtcNow,
			DateTime.UtcNow);

		var projectDto = new ProjectDetailDto(
			projectId,
			"Test Project",
			null,
			userId, // User owns the project
			DateTime.UtcNow,
			DateTime.UtcNow,
			new List<TaskDto>());

		_mockMediator
			.Setup(m => m.Send(It.Is<GetTaskByIdQuery>(q => q.Id == taskId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(taskDto);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetProjectByIdQuery>(q => q.Id == projectId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(projectDto);

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeTrue();
		context.HasFailed.Should().BeFalse();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenUserDoesNotOwnProjectContainingTask_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var ownerId = "differentUser";
		var projectId = Guid.NewGuid();
		var taskId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = taskId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		var taskDto = new TaskDto(
			taskId,
			"Test Task",
			null,
			null,
			ProjectTaskStatus.Todo,
			Priority.Medium,
			null,
			projectId,
			null,
			DateTime.UtcNow,
			DateTime.UtcNow);

		var projectDto = new ProjectDetailDto(
			projectId,
			"Test Project",
			null,
			ownerId, // Different owner
			DateTime.UtcNow,
			DateTime.UtcNow,
			new List<TaskDto>());

		_mockMediator
			.Setup(m => m.Send(It.Is<GetTaskByIdQuery>(q => q.Id == taskId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(taskDto);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetProjectByIdQuery>(q => q.Id == projectId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(projectDto);

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenTaskNotFound_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var taskId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = taskId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetTaskByIdQuery>(q => q.Id == taskId), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new KeyNotFoundException("Task not found"));

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenNoTaskIdInRoute_ShouldSucceed()
	{
		// Arrange - No task ID in route (doesn't apply)
		var userId = "user123";

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		// No route value for "id"

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		// Act
		await _handler.HandleAsync(context);

		// Assert - Should succeed because policy doesn't apply
		context.HasSucceeded.Should().BeTrue();
		context.HasFailed.Should().BeFalse();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenUserIdNotInClaims_ShouldFail()
	{
		// Arrange
		var taskId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = taskId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenHttpContextIsNull_ShouldFail()
	{
		// Arrange
		var userId = "user123";

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns((HttpContext?)null);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenProjectNotFound_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var projectId = Guid.NewGuid();
		var taskId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = taskId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new TaskOwnershipRequirement() },
			user,
			null);

		var taskDto = new TaskDto(
			taskId,
			"Test Task",
			null,
			null,
			ProjectTaskStatus.Todo,
			Priority.Medium,
			null,
			projectId,
			null,
			DateTime.UtcNow,
			DateTime.UtcNow);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetTaskByIdQuery>(q => q.Id == taskId), It.IsAny<CancellationToken>()))
			.ReturnsAsync(taskDto);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetProjectByIdQuery>(q => q.Id == projectId), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new KeyNotFoundException("Project not found"));

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}
}
