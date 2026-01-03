using FocusFlow.Application.Features.Dashboard.Common;
using FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;
using FocusFlow.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Controller for dashboard statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<DashboardController> _logger;

	public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Get dashboard statistics for the current user's projects
	/// </summary>
	/// <returns>List of project statistics</returns>
	[HttpGet("statistics")]
	[ProducesResponseType(typeof(List<ProjectStatisticsDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<List<ProjectStatisticsDto>>> GetStatistics()
	{
		var userId = GetCurrentUserId();
		_logger.LogInformation("User {UserId} is retrieving dashboard statistics", userId);

		var query = new GetDashboardStatisticsQuery(userId);
		var result = await _mediator.Send(query);

		_logger.LogInformation("Retrieved dashboard statistics for user {UserId}: {ProjectCount} projects", 
			userId, result.Count);

		return Ok(result);
	}

	private string GetCurrentUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new FocusFlowUnauthorizedException("User ID not found in token");
	}
}
