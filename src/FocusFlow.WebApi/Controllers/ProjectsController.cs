using FocusFlow.Application.DTOs;
using FocusFlow.Application.Projects.Commands;
using FocusFlow.Application.Projects.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Projects management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly IMediator _mediator;

	public ProjectsController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Get all projects
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ProjectDto>>> GetAll(CancellationToken cancellationToken)
	{
		var query = new GetAllProjectsQuery();
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Get project by ID
	/// </summary>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var query = new GetProjectByIdQuery(id);
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Get projects by owner ID
	/// </summary>
	[HttpGet("owner/{ownerId}")]
	[ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<ProjectDto>>> GetByOwner(string ownerId, CancellationToken cancellationToken)
	{
		var query = new GetProjectsByOwnerQuery(ownerId);
		var result = await _mediator.Send(query, cancellationToken);
		return Ok(result);
	}

	/// <summary>
	/// Create a new project
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual user ID from authentication context
		var userId = "temp-user-id";
		
		var command = new CreateProjectCommand(dto.Name, dto.Description, userId);
		var result = await _mediator.Send(command, cancellationToken);
		
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	/// <summary>
	/// Update an existing project
	/// </summary>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
	{
		var command = new UpdateProjectCommand(id, dto.Name, dto.Description);
		await _mediator.Send(command, cancellationToken);
		
		return NoContent();
	}

	/// <summary>
	/// Delete a project
	/// </summary>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var command = new DeleteProjectCommand(id);
		await _mediator.Send(command, cancellationToken);
		
		return NoContent();
	}
}