using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Application.Features.Tasks.GetTaskById;
using FocusFlow.Application.Features.Tasks.UpdateTaskStatus;
using FocusFlow.Application.Features.Tasks.DeleteTask;
using FocusFlow.Application.Features.Tasks.AssignTask;
using FocusFlow.Application.Features.Tasks.GetTasksByOwnerAndFilter;
using FocusFlow.Application.Features.Tasks.UpdateTask;
using FocusFlow.Application.Features.Tasks.UnassignTask;
using FocusFlow.Application.Features.Tasks.GetTasksByUser;
using FocusFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Controller for managing tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<TasksController> _logger;

	public TasksController(IMediator mediator, ILogger<TasksController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Create a new task in a project
	/// </summary>
	/// <param name="projectId">Project ID</param>
	/// <param name="dto">Task creation data</param>
	/// <returns>Created task</returns>
	[HttpPost]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto)
	{
		var userId = GetCurrentUserId();
		var projectId = dto.ProjectId;
		_logger.LogInformation("User {UserId} attempting to create task '{TaskTitle}' in project {ProjectId}",
			userId, dto.Title, projectId);

		// Verify user owns the project
		var projectQuery = new GetProjectByIdQuery(projectId);
		var project = await _mediator.Send(projectQuery);

		if (project.OwnerId != userId)
		{
			_logger.LogWarning("User {UserId} attempted to create task in project {ProjectId} owned by {OwnerId}",
				userId, projectId, project.OwnerId);
			return Forbid();
		}

		var command = new CreateTaskCommand(
			projectId,
			dto.Title,
			dto.Description,
			dto.DueDate,
			dto.Priority,
			dto.AssignedUserId);

		var result = await _mediator.Send(command);

		_logger.LogInformation("User {UserId} successfully created task {TaskId} in project {ProjectId}",
			userId, result.Id, projectId);

		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Update an existing task
	/// </summary>
	[HttpPut("{id}")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskRequest request)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is updating task {TaskId}", userId, id);

		var command = new UpdateTaskCommand(id, request.Title, request.Description, request.DueDate, request.Priority);
		var result = await _mediator.Send(command);

		_logger.LogInformation("User {UserId} successfully updated task {TaskId}", userId, id);
		return Ok(result);
	}

	/// <summary>
	/// Unassign a task
	/// </summary>
	[HttpPatch("{id}/unassign")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TaskDto>> Unassign(Guid id)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is unassigning task {TaskId}", userId, id);

		var result = await _mediator.Send(new UnassignTaskCommand(id));
		_logger.LogInformation("User {UserId} successfully unassigned task {TaskId}", userId, id);
		return Ok(result);
	}

	/// <summary>
	/// Get tasks assigned to a specific user
	/// </summary>
	[HttpGet("user/{userId}")]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TaskDto>>> GetByUser(string userId)
	{
		_logger.LogInformation("Retrieving tasks for user {UserId}", userId);

		var result = await _mediator.Send(new GetTasksByUserQuery(userId));

		_logger.LogInformation("Retrieved {Count} tasks for user {UserId}", result.Count, userId);
		return Ok(result);
	}

	/// <summary>
	/// Get a specific task by ID
	/// </summary>
	/// <param name="id">Task ID</param>
	/// <returns>Task details</returns>
	[HttpGet("{id}")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TaskDto>> GetById(Guid id)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is retrieving task {TaskId}", userId, id);

		var task = await _mediator.Send(new GetTaskByIdQuery(id));

		_logger.LogInformation("User {UserId} successfully retrieved task {TaskId}", userId, id);
		return Ok(task);
	}

	/// <summary>
	/// Update task status
	/// </summary>
	/// <param name="id">Task ID</param>
	/// <param name="request">New status</param>
	/// <returns>Updated task</returns>
	[HttpPatch("{id}/status")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TaskDto>> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is updating status of task {TaskId} to {Status}",
			userId, id, request.Status);

		var command = new UpdateTaskStatusCommand(id, request.Status);
		var result = await _mediator.Send(command);

		_logger.LogInformation("User {UserId} successfully updated task {TaskId} status to {Status}",
			userId, id, request.Status);

		return Ok(result);
	}

	/// <summary>
	/// Delete a task
	/// </summary>
	/// <param name="id">Task ID</param>
	/// <returns>No content</returns>
	[HttpDelete("{id}")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Delete(Guid id)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is deleting task {TaskId}", userId, id);

		await _mediator.Send(new DeleteTaskCommand(id));

		_logger.LogInformation("User {UserId} successfully deleted task {TaskId}", userId, id);
		return NoContent();
	}

	/// <summary>
	/// Assign task to a user
	/// </summary>
	/// <param name="id">Task ID</param>
	/// <param name="request">User assignment data</param>
	/// <returns>Updated task</returns>
	[HttpPatch("{id}/assign")]
	[Authorize(Policy = "TaskOwner")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<TaskDto>> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is assigning task {TaskId} to user {AssignedUserId}",
			userId, id, request.UserId);

		var result = await _mediator.Send(new AssignTaskCommand(id, request.UserId));

		_logger.LogInformation("User {UserId} successfully assigned task {TaskId} to user {AssignedUserId}",
			userId, id, request.UserId);

		return Ok(result);
	}

	/// <summary>
	/// Get tasks filtered by criteria
	/// </summary>
	/// <param name="status">Filter by status</param>
	/// <param name="priority">Filter by priority</param>
	/// <param name="isOverdue">Filter overdue tasks</param>
	/// <returns>Filtered tasks</returns>
	[HttpGet]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TaskDto>>> GetFiltered(
		[FromQuery] ProjectTaskStatus? status = null,
		[FromQuery] Priority? priority = null,
		[FromQuery] bool? isOverdue = null)
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is retrieving filtered tasks with status: {Status}, priority: {Priority}, overdue: {IsOverdue}",
			userId, status, priority, isOverdue);

		var query = new GetTasksByOwnerAndFilterQuery(userId, status, priority, isOverdue);
		var userTasks = await _mediator.Send(query);

		_logger.LogInformation("Retrieved {Count} filtered tasks for user {UserId}", userTasks.Count, userId);

		return Ok(userTasks);
	}

	private string GetCurrentUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException("User ID not found in token");
	}
}

/// <summary>
/// Request model for updating task status
/// </summary>
public class UpdateTaskStatusRequest
{
	public ProjectTaskStatus Status { get; set; }
}

/// <summary>
/// Request model for assigning task to user
/// </summary>
public class AssignTaskRequest
{
	public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a task
/// </summary>
public class UpdateTaskRequest
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTime? DueDate { get; set; }
	public Priority Priority { get; set; }
}