using FocusFlow.Application.Features.Authentication.Commands.Login;
using FocusFlow.Application.Features.Authentication.Commands.Register;
using FocusFlow.Application.Features.Authentication.Common;
using FocusFlow.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Controller for authentication and user management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISender mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <returns>Success message</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation("Register attempt for email: {Email}", request.Email);

        var command = new RegisterCommand(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName,
            correlationId);
        
        var result = await _mediator.Send(command);

        _logger.LogInformation("User registered: {Email}", result.Email);

        return Ok(result);
    }

    /// <summary>
    /// Login user and get JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var command = new LoginCommand(request.Email, request.Password, correlationId);
        var result = await _mediator.Send(command);

        _logger.LogInformation("User logged in: {Email}", result.Email);

        return Ok(result);
    }
}