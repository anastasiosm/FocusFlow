using FocusFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FocusFlow.WebApi.Controllers;

/// <summary>
/// Controller for authentication and user management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly IConfiguration _configuration;
	private readonly ILogger<AuthController> _logger;

	public AuthController(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		IConfiguration configuration,
		ILogger<AuthController> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_configuration = configuration;
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
		// Check if user already exists
		var existingUser = await _userManager.FindByEmailAsync(request.Email);
		if (existingUser != null)
		{
			return BadRequest(new { message = "User with this email already exists" });
		}

		// Create new user
		var user = new ApplicationUser
		{
			UserName = request.Email,
			Email = request.Email,
			FirstName = request.FirstName,
			LastName = request.LastName,
			EmailConfirmed = true // Auto-confirm for demo purposes
		};

		var result = await _userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			_logger.LogWarning("User registration failed: {Errors}", errors);
			return BadRequest(new { message = "Registration failed", errors = result.Errors });
		}

		_logger.LogInformation("New user registered: {Email}", request.Email);

		// Automatically log in the user after registration
		var token = await GenerateJwtToken(user);

		return Ok(new AuthResponse
		{
			Token = token,
			UserName = user.FullName,
			Email = user.Email!,
			Expiration = DateTime.UtcNow.AddHours(GetTokenExpirationHours())
		});
	}

	/// <summary>
	/// Login user and get JWT token
	/// </summary>
	/// <param name="request">Login credentials</param>
	/// <returns>JWT token</returns>
	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		if (user == null)
		{
			_logger.LogWarning("Login attempt failed: User not found - {Email}", request.Email);
			return Unauthorized(new { message = "Invalid email or password" });
		}

		var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

		if (!result.Succeeded)
		{
			_logger.LogWarning("Login attempt failed: Invalid password - {Email}", request.Email);
			return Unauthorized(new { message = "Invalid email or password" });
		}

		var token = await GenerateJwtToken(user);

		_logger.LogInformation("User logged in successfully: {Email}", request.Email);

		return Ok(new AuthResponse
		{
			Token = token,
			UserName = user.FullName,
			Email = user.Email!,
			Expiration = DateTime.UtcNow.AddHours(GetTokenExpirationHours())
		});
	}

	/// <summary>
	/// Get current user information
	/// </summary>
	/// <returns>User details</returns>
	[HttpGet("me")]
	[ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<UserInfoResponse>> GetCurrentUser()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			return Unauthorized();
		}

		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return Unauthorized();
		}

		return Ok(new UserInfoResponse
		{
			Id = user.Id,
			Email = user.Email!,
			FirstName = user.FirstName ?? "",
			LastName = user.LastName ?? "",
			FullName = user.FullName
		});
	}

	private async Task<string> GenerateJwtToken(ApplicationUser user)
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.UserName!),
			new Claim(ClaimTypes.Email, user.Email!),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
			_configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));

		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["JwtSettings:Issuer"],
			audience: _configuration["JwtSettings:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddHours(GetTokenExpirationHours()),
			signingCredentials: credentials
		);

		return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
	}

	private int GetTokenExpirationHours()
	{
		return int.TryParse(_configuration["JwtSettings:ExpirationHours"], out var hours) ? hours : 24;
	}
}

// Request/Response Models
public class RegisterRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
	public string Token { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public DateTime Expiration { get; set; }
}

public class UserInfoResponse
{
	public string Id { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
}