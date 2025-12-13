using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.GetTaskById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FocusFlow.WebApi.Authorization.TaskOwnership;

/// <summary>
/// Validates that the current user owns the project containing the task specified in the route
/// </summary>
public class TaskOwnershipHandler : AuthorizationHandler<TaskOwnershipRequirement>
{
	private readonly IMediator _mediator;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<TaskOwnershipHandler> _logger;

	public TaskOwnershipHandler(
		IMediator mediator,
		IHttpContextAccessor httpContextAccessor,
		ILogger<TaskOwnershipHandler> logger)
	{
		_mediator = mediator;
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		TaskOwnershipRequirement requirement)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext == null)
		{
			_logger.LogWarning("HttpContext is null in TaskOwnershipHandler");
			context.Fail();
			return;
		}

		// Get taskId from route parameter 'id'
		if (!httpContext.Request.RouteValues.TryGetValue("id", out var idValue) ||
			!Guid.TryParse(idValue?.ToString(), out var taskId))
		{
			// No task ID in route - this policy doesn't apply
			context.Succeed(requirement);
			return;
		}

		var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			_logger.LogWarning("User ID not found in claims for task {TaskId}", taskId);
			context.Fail();
			return;
		}

		try
		{
			// Get task to find its project
			var taskQuery = new GetTaskByIdQuery(taskId);
			var task = await _mediator.Send(taskQuery);

			// Get project to check ownership
			var projectQuery = new GetProjectByIdQuery(task.ProjectId);
			var project = await _mediator.Send(projectQuery);

			if (project.OwnerId == userId)
			{
				_logger.LogDebug("User {UserId} authorized for task {TaskId} in project {ProjectId}",
					userId, taskId, task.ProjectId);
				context.Succeed(requirement);
			}
			else
			{
				_logger.LogWarning("User {UserId} denied access to task {TaskId} in project {ProjectId} owned by {OwnerId}",
					userId, taskId, task.ProjectId, project.OwnerId);
				context.Fail();
			}
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning(ex, "Task {TaskId} or its project not found during authorization", taskId);
			context.Fail();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during task ownership authorization for task {TaskId}", taskId);
			context.Fail();
		}
	}
}