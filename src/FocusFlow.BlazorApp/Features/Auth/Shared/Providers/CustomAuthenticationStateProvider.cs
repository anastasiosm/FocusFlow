using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Auth;

/// <summary>
/// Provides an implementation of <see cref="AuthenticationStateProvider"/> that manages authentication state using JWT
/// tokens and a custom token provider.
/// </summary>
/// <remarks>This provider retrieves, validates, and parses JWT tokens to determine the current user's
/// authentication state. It supports marking users as authenticated or logged out by updating the stored token and
/// notifying consumers of authentication state changes. This class is typically used in Blazor applications to
/// integrate custom authentication logic with the built-in authentication system.</remarks>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<CustomAuthenticationStateProvider> _logger;

	public CustomAuthenticationStateProvider(
		ITokenProvider tokenProvider,
		ILogger<CustomAuthenticationStateProvider> logger)
	{
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

	/// <summary>
	/// Asynchronously asks the injected ITokenProvider for a JWT (GetTokenAsync()).
	/// If a valid JWT token is found, parses it to create an authenticated ClaimsPrincipal.
	/// If no token or an expired/invalid token is found, returns an anonymous ClaimsPrincipal.
	/// 
	/// Blazor calls this method (when it needs) to get the current user's authentication state.
	/// The provider determines whether the app treats the user as authenticated and which claims/roles they have.
	/// 
	/// Extra: The method uses async/await to avoid blocking the UI thread during token retrieval and validation.
	/// </summary>	
	/// <returns>Task<AuthenticationState> — asynchronous task that yields an AuthenticationState (which wraps a ClaimsPrincipal) 
	/// based on the stored JWT token.</returns>
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		// Get token using async method
		var token = await _tokenProvider.GetTokenAsync();

		if (string.IsNullOrWhiteSpace(token))
		{
			return CreateAnonymousState();
		}

		// Validate token expiration
		if (IsTokenExpired(token))
		{
			_logger.LogInformation("Token has expired");
			await _tokenProvider.ClearTokenAsync();
			return CreateAnonymousState();
		}

		try
		{
			var claims = JwtParser.ParseClaimsFromJwt(token);
			var identity = new ClaimsIdentity(claims, "jwt");
			var user = new ClaimsPrincipal(identity);

			return new AuthenticationState(user);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error parsing JWT token");
			await _tokenProvider.ClearTokenAsync();
			return CreateAnonymousState();
		}
	}

	/// <summary>
	/// update the token and notify consumers via NotifyAuthenticationStateChanged that the user is now authenticated.
	/// </summary>	
	public async Task MarkUserAsAuthenticatedAsync(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new ArgumentException("Token cannot be null or empty", nameof(token));
		}

		try
		{
			// Store token using async method
			await _tokenProvider.SetTokenAsync(token);

			// Parse claims and update auth state
			var claims = JwtParser.ParseClaimsFromJwt(token);
			var identity = new ClaimsIdentity(claims, "jwt");
			var user = new ClaimsPrincipal(identity);

			var authState = new AuthenticationState(user);
			NotifyAuthenticationStateChanged(Task.FromResult(authState));

			_logger.LogInformation("User authenticated successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error marking user as authenticated");
			throw;
		}
	}

	/// <summary>
	/// update the token and notify consumers via NotifyAuthenticationStateChanged that the user is now logged out.
	/// </summary>	
	public async Task MarkUserAsLoggedOutAsync()
	{
		await _tokenProvider.ClearTokenAsync();
		NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState()));
		_logger.LogInformation("User logged out");
	}

	/// <summary>
	/// IsTokenExpired checks if the provided JWT token has expired.
	/// </summary>
	private bool IsTokenExpired(string token)
	{
		try
		{
			var claims = JwtParser.ParseClaimsFromJwt(token);
			var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

			if (string.IsNullOrEmpty(expClaim))
			{
				return false;
			}

			var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim));
			return expDateTime.UtcDateTime <= DateTime.UtcNow;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error checking token expiration");
			return true;
		}
	}

	/// <summary>
	/// Creates an anonymous authentication state representing an unauthenticated user.
	/// </summary>
	private static AuthenticationState CreateAnonymousState()
	{
		return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
	}
}