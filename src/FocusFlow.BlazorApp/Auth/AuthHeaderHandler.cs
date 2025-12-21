using System.Net.Http.Headers;

namespace FocusFlow.BlazorApp.Auth;

public class AuthHeaderHandler : DelegatingHandler
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<AuthHeaderHandler> _logger;

	public AuthHeaderHandler(ITokenProvider tokenProvider, ILogger<AuthHeaderHandler> logger)
	{
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

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

		// Get token from provider (synchronous, no localStorage needed)
		var token = _tokenProvider.GetToken();

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