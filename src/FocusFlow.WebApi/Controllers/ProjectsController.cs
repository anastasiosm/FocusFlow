using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Features.Projects.GetProjectsByOwner;
using FocusFlow.Application.Features.Projects.GetAllProjects;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Projects.DeleteProject;
using FocusFlow.Application.Features.Tasks.Common;
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
	/// Get all projects (admin/dev)
	/// </summary>
	[HttpGet("all")]
	[ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
	{
		var result = await _mediator.Send(new GetAllProjectsQuery());
		_logger.LogInformation("Retrieved {Count} total projects", result.Count);
		return Ok(result);
	}

	/// <summary>
	/// Get a specific project by ID
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <returns>Project details with tasks</returns>
	[HttpGet("{id}")]
	[Authorize(Policy = "ProjectOwner")]
	[ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id)
	{
		var query = new GetProjectByIdQuery(id);
		var result = await _mediator.Send(query);

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

		_logger.LogInformation("Creating project: {ProjectName} for user {UserId}",	dto.Name, userId);

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
	[Authorize(Policy = "ProjectOwner")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
	{
		var userId = GetCurrentUserId();
		var command = new UpdateProjectCommand(id, dto.Name, dto.Description, userId);
		
		await _mediator.Send(command);

		_logger.LogInformation("User {UserId} updated project {ProjectId}", userId, id);
		
		return NoContent();
	}

	/// <summary>
	/// Delete a project
	/// </summary>
	/// <param name="id">Project ID</param>
	/// <returns>No content</returns>
	[HttpDelete("{id}")]
	[Authorize(Policy = "ProjectOwner")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Delete(Guid id)
	{
		var userId = GetCurrentUserId();
		var command = new DeleteProjectCommand(id, userId);
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
	[Authorize(Policy = "ProjectOwner")]
	[ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<ActionResult<List<TaskDto>>> GetProjectTasks(Guid id)
	{
		var query = new GetProjectByIdQuery(id);
		var project = await _mediator.Send(query);

		return Ok(project.Tasks);
	}

	private string GetCurrentUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException("User ID not found in token");
	}
}