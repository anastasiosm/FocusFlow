using FocusFlow.Application.Features.Projects.GetProjectById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FocusFlow.WebApi.Authorization.ProjectOwnership;

/// <summary>
/// Validates that the current user owns the project specified in the route
/// </summary>
public class ProjectOwnershipHandler : AuthorizationHandler<ProjectOwnershipRequirement>
{
	private readonly IMediator _mediator;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<ProjectOwnershipHandler> _logger;

	public ProjectOwnershipHandler(
		IMediator mediator,
		IHttpContextAccessor httpContextAccessor,
		ILogger<ProjectOwnershipHandler> logger)
	{
		_mediator = mediator;
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ProjectOwnershipRequirement requirement)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext == null)
		{
			_logger.LogWarning("HttpContext is null in ProjectOwnershipHandler");
			context.Fail();
			return;
		}

		// Get projectId from route parameter 'id'
		if (!httpContext.Request.RouteValues.TryGetValue("id", out var idValue) ||
			!Guid.TryParse(idValue?.ToString(), out var projectId))
		{
			// No project ID in route - this policy doesn't apply
			context.Succeed(requirement);
			return;
		}

		var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			_logger.LogWarning("User ID not found in claims for project {ProjectId}", projectId);
			context.Fail();
			return;
		}

		try
		{
			var query = new GetProjectByIdQuery(projectId);
			var project = await _mediator.Send(query);

			if (project.OwnerId == userId)
			{
				_logger.LogDebug("User {UserId} authorized for project {ProjectId}", userId, projectId);
				context.Succeed(requirement);
			}
			else
			{
				_logger.LogWarning("User {UserId} denied access to project {ProjectId} owned by {OwnerId}",
					userId, projectId, project.OwnerId);
				context.Fail();
			}
		}
		catch (KeyNotFoundException)
		{
			_logger.LogWarning("Project {ProjectId} not found during authorization", projectId);
			context.Fail();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during project ownership authorization for project {ProjectId}", projectId);
			context.Fail();
		}
	}
}