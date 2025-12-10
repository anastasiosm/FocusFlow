using FocusFlow.Application.DTO;
using FocusFlow.Application.Projects.Commands;
using FocusFlow.Application.Projects.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Controller for managing projects
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<ProjectsController> _logger;

	public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Get all projects for the current user
	/// </summary>
	/// <returns>List of projects</returns>
	[HttpGet]
	[ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<List<ProjectDto>>> GetAll()
	{
		var userId = GetCurrentUserId();
		var query = new GetProjectsByOwnerQuery(userId);
		var result = await _mediator.Send(query);

		_logger.LogInformation("Retrieved {Count} projects for user {UserId}", result.Count, userId);
		return Ok(result);
	}

	/// <summary>
	/// Get a specific project by ID
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <returns>Project details with tasks</returns>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id)
	{
		var query = new GetProjectByIdQuery(id);
		var result = await _mediator.Send(query);

		// Check if user owns this project
		var userId = GetCurrentUserId();
		if (result.OwnerId != userId)
		{
			_logger.LogWarning("User {UserId} attempted to access project {ProjectId} owned by {OwnerId}",
				userId, id, result.OwnerId);
			return Forbid();
		}

		return Ok(result);
	}

	/// <summary>
	/// Create a new project
	/// </summary>
	/// <param name="dto">Project creation data</param>
	/// <returns>Created project</returns>
	[HttpPost]
	[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto)
	{
		var userId = GetCurrentUserId();
		var command = new CreateProjectCommand(dto.Name, dto.Description, userId);
		var result = await _mediator.Send(command);

		_logger.LogInformation("User {UserId} created project {ProjectId}", userId, result.Id);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Update an existing project
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <param name="dto">Updated project data</param>
	/// <returns>Updated project</returns>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto dto)
	{
		// First check if project exists and user owns it
		var existingQuery = new GetProjectByIdQuery(id);
		var existing = await _mediator.Send(existingQuery);

		var userId = GetCurrentUserId();
		if (existing.OwnerId != userId)
		{
			_logger.LogWarning("User {UserId} attempted to update project {ProjectId} owned by {OwnerId}",
				userId, id, existing.OwnerId);
			return Forbid();
		}

		var command = new UpdateProjectCommand(id, dto.Name, dto.Description);
		var result = await _mediator.Send(command);

		_logger.LogInformation("User {UserId} updated project {ProjectId}", userId, id);
		return Ok(result);
	}

	/// <summary>
	/// Delete a project
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <returns>No content</returns>
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Delete(Guid id)
	{
		// First check if project exists and user owns it
		var existingQuery = new GetProjectByIdQuery(id);
		var existing = await _mediator.Send(existingQuery);

		var userId = GetCurrentUserId();
		if (existing.OwnerId != userId)
		{
			_logger.LogWarning("User {UserId} attempted to delete project {ProjectId} owned by {OwnerId}",
				userId, id, existing.OwnerId);
			return Forbid();
		}

		var command = new DeleteProjectCommand(id);
		await _mediator.Send(command);

		_logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, id);
		return NoContent();
	}

	/// <summary>
	/// Get all tasks for a specific project
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <returns>List of tasks</returns>
	[HttpGet("{id}/tasks")]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<List<TaskDto>>> GetProjectTasks(Guid id)
	{
		var query = new GetProjectByIdQuery(id);
		var project = await _mediator.Send(query);

		var userId = GetCurrentUserId();
		if (project.OwnerId != userId)
		{
			return Forbid();
		}

		return Ok(project.Tasks);
	}

	private string GetCurrentUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException("User ID not found in token");
	}
}