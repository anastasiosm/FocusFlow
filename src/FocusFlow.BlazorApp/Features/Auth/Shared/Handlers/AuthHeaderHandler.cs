using FocusFlow.BlazorApp.Services; 
using System.Net.Http.Headers; 

namespace FocusFlow.BlazorApp.Auth;

/// <summary>
/// HTTP handler that adds Authorization header with Bearer token to outgoing requests. 
/// 
/// •	Inheriting DelegatingHandler makes this class an HTTP message handler that can participate in an HttpClient pipeline. 
///		It can inspect/modify outgoing HttpRequestMessages and incoming HttpResponseMessages by overriding SendAsync(HttpRequestMessage, CancellationToken).
/// •	Because it is public, it can be registered with dependency injection and reused across the app
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<AuthHeaderHandler> _logger;

	public AuthHeaderHandler(ITokenProvider tokenProvider, ILogger<AuthHeaderHandler> logger)
	{
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

	/// <summary>
	/// Adds a Bearer Authorization header to outgoing HTTP requests when a token is available (via ITokenProvider) 
	/// and skips certain endpoints or requests that already have an Authorization header.
	/// </summary>	
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		_logger.LogWarning("🔥 AuthHeaderHandler CALLED for {Url}", request.RequestUri);

		// Skip if Authorization header already exists
		if (request.Headers.Authorization != null)
		{
			_logger.LogInformation("✅ Authorization header already exists");
			return await base.SendAsync(request, cancellationToken);
		}

		// Skip authentication endpoints - they don't need/shouldn't have Authorization header
		var path = request.RequestUri?.AbsolutePath ?? string.Empty;
		if (path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase) ||
			path.Contains("/auth/register", StringComparison.OrdinalIgnoreCase))
		{
			_logger.LogInformation("⏭️ Skipping Authorization header for auth endpoint: {Path}", path);
			return await base.SendAsync(request, cancellationToken);
		}

		// Get token from provider (async)
		var token = await _tokenProvider.GetTokenAsync();
		if (string.IsNullOrWhiteSpace(token))
		{
			_logger.LogDebug("⚠️ No token available, skipping Authorization header");
			return await base.SendAsync(request, cancellationToken);
		}

		_logger.LogWarning("🔥 Token from provider: {HasToken}, Length: {Length}",
			!string.IsNullOrWhiteSpace(token),
			token?.Length ?? 0);

		if (!string.IsNullOrWhiteSpace(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			_logger.LogWarning("🔥 Authorization header ADDED");
		}
		else
		{
			_logger.LogDebug("⚠️ No token available for request to {Url}", request.RequestUri);
		}

		return await base.SendAsync(request, cancellationToken);
	}
}