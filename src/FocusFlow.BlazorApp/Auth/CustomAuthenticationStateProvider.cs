using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FocusFlow.BlazorApp.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILocalStorageService _localStorage;
	private readonly ILogger<CustomAuthenticationStateProvider> _logger;

	public CustomAuthenticationStateProvider(
		ITokenProvider tokenProvider,
		ILocalStorageService localStorage,
		ILogger<CustomAuthenticationStateProvider> logger)
	{
		_tokenProvider = tokenProvider;
		_localStorage = localStorage;
		_logger = logger;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		// Initialize token from localStorage if needed
		await _tokenProvider.InitializeAsync(_localStorage);

		// Get token from provider
		var token = _tokenProvider.GetToken();

		if (string.IsNullOrWhiteSpace(token))
		{
			return CreateAnonymousState();
		}

		// Validate token expiration
		if (IsTokenExpired(token))
		{
			_logger.LogInformation("Token has expired");
			await _tokenProvider.ClearTokenAsync(_localStorage);
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
			await _tokenProvider.ClearTokenAsync(_localStorage);
			return CreateAnonymousState();
		}
	}

	public async Task MarkUserAsAuthenticatedAsync(string token, ILocalStorageService localStorage)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new ArgumentException("Token cannot be null or empty", nameof(token));
		}

		try
		{
			// Store token in provider (in-memory + localStorage)
			await _tokenProvider.SetTokenAsync(token, localStorage);

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

	public async Task MarkUserAsLoggedOutAsync(ILocalStorageService localStorage)
	{
		await _tokenProvider.ClearTokenAsync(localStorage);
		NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState()));
		_logger.LogInformation("User logged out");
	}

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

	private static AuthenticationState CreateAnonymousState()
	{
		return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
	}
}