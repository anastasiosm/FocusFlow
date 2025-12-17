using FluentAssertions;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Exceptions;
using FocusFlow.WebApi.Authorization.ProjectOwnership;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FocusFlow.WebApi.Tests.Authorization;

public class ProjectOwnershipHandlerTests
{
	private readonly Mock<IMediator> _mockMediator;
	private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
	private readonly Mock<ILogger<ProjectOwnershipHandler>> _mockLogger;
	private readonly ProjectOwnershipHandler _handler;

	public ProjectOwnershipHandlerTests()
	{
		_mockMediator = new Mock<IMediator>();
		_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
		_mockLogger = new Mock<ILogger<ProjectOwnershipHandler>>();

		_handler = new ProjectOwnershipHandler(
			_mockMediator.Object,
			_mockHttpContextAccessor.Object,
			_mockLogger.Object);
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenUserOwnsProject_ShouldSucceed()
	{
		// Arrange
		var userId = "user123";
		var projectId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = projectId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new ProjectOwnershipRequirement() },
			user,
			null);

		var projectDto = new ProjectDetailDto(
			projectId,
			"Test Project",
			null,
			userId, // User owns this project
			DateTime.UtcNow,
			DateTime.UtcNow,
			new List<TaskDto>());

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
	public async Task HandleRequirementAsync_WhenUserDoesNotOwnProject_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var ownerId = "differentUser";
		var projectId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = projectId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new ProjectOwnershipRequirement() },
			user,
			null);

		var projectDto = new ProjectDetailDto(
			projectId,
			"Test Project",
			null,
			ownerId, // Different owner
			DateTime.UtcNow,
			DateTime.UtcNow,
			new List<TaskDto>());

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
	public async Task HandleRequirementAsync_WhenProjectNotFound_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var projectId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = projectId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new ProjectOwnershipRequirement() },
			user,
			null);

		_mockMediator
			.Setup(m => m.Send(It.Is<GetProjectByIdQuery>(q => q.Id == projectId), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new KeyNotFoundException("Project not found"));

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenNoProjectIdInRoute_ShouldSucceed()
	{
		// Arrange - No project ID in route (doesn't apply)
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
			new[] { new ProjectOwnershipRequirement() },
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
		var projectId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = projectId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new ProjectOwnershipRequirement() },
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
			new[] { new ProjectOwnershipRequirement() },
			user,
			null);

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}

	[Fact]
	public async Task HandleRequirementAsync_WhenExceptionThrown_ShouldFail()
	{
		// Arrange
		var userId = "user123";
		var projectId = Guid.NewGuid();

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuth"));

		var httpContext = new DefaultHttpContext();
		httpContext.Request.RouteValues["id"] = projectId.ToString();

		_mockHttpContextAccessor
			.Setup(x => x.HttpContext)
			.Returns(httpContext);

		var context = new AuthorizationHandlerContext(
			new[] { new ProjectOwnershipRequirement() },
			user,
			null);

		_mockMediator
			.Setup(m => m.Send(It.IsAny<GetProjectByIdQuery>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act
		await _handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
		context.HasFailed.Should().BeTrue();
	}
}
