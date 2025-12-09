using FocusFlow.Application.DTO;
using FocusFlow.Application.Tasks.Commands;
using FocusFlow.Application.Tasks.Queries;
using FocusFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Tasks management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
	private readonly IMediator _mediator;

	public TasksController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Get tasks with optional filters
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TaskDto>>> GetFiltered(
		[FromQuery] ProjectTaskStatus? status,
		[FromQuery] Priority? priority,
		[FromQuery] bool? isOverdue,
		CancellationToken cancellationToken)
	{
		var query = new GetTasksByFilterQuery(status, priority, isOverdue);
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Get tasks by project ID
	/// </summary>
	[HttpGet("project/{projectId:guid}")]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TaskDto>>> GetByProject(Guid projectId, CancellationToken cancellationToken)
	{
		var query = new GetTasksByProjectQuery(projectId);
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Get tasks assigned to a user
	/// </summary>
	[HttpGet("user/{userId}")]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TaskDto>>> GetByUser(string userId, CancellationToken cancellationToken)
	{
		var query = new GetTasksByUserQuery(userId);
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Create a new task
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
	{
		var command = new CreateTaskCommand(
			dto.ProjectId,
			dto.Title,
			dto.Description,
			dto.DueDate,
			dto.Priority,
			dto.AssignedUserId);
		
		var result = await _mediator.Send(command, cancellationToken);
		
		return CreatedAtAction(nameof(GetByProject), new { projectId = result.ProjectId }, result);
	}

	/// <summary>
	/// Update an existing task
	/// </summary>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
	{
		var command = new UpdateTaskCommand(
			id,
			dto.Title,
			dto.Description,
			dto.DueDate,
			dto.Priority);
		
		var result = await _mediator.Send(command, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Update task status
	/// </summary>
	[HttpPatch("{id:guid}/status")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto dto, CancellationToken cancellationToken)
	{
		var command = new UpdateTaskStatusCommand(id, dto.Status);
		var result = await _mediator.Send(command, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Assign task to a user
	/// </summary>
	[HttpPatch("{id:guid}/assign")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> Assign(Guid id, [FromBody] AssignTaskDto dto, CancellationToken cancellationToken)
	{
		var command = new AssignTaskCommand(id, dto.UserId);
		var result = await _mediator.Send(command, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Unassign task from user
	/// </summary>
	[HttpPatch("{id:guid}/unassign")]
	[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<TaskDto>> Unassign(Guid id, CancellationToken cancellationToken)
	{
		var command = new UnassignTaskCommand(id);
		var result = await _mediator.Send(command, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Delete a task
	/// </summary>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var command = new DeleteTaskCommand(id);
		await _mediator.Send(command, cancellationToken);
		
		return NoContent();
	}
}