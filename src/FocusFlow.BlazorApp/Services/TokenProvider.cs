using Blazored.LocalStorage;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// TokenProvider that uses a static ConcurrentDictionary to share state across all instances.
/// This implementation DOES NOT depend on scoped services in the constructor so it can be registered as a Singleton.
/// Persistence operations that require the scoped ILocalStorageService are performed via method parameters.
/// </summary>
public class TokenProvider : ITokenProvider
{
	// Static storage shared across all instances - keyed by circuit ID
	private static readonly ConcurrentDictionary<string, string> _tokens = new();

	private readonly ILogger<TokenProvider> _logger;
	private readonly string _circuitId;
	private const string TokenKey = "authToken";

	public TokenProvider(ILogger<TokenProvider> logger)
	{
		_logger = logger;
		// Simple circuit id placeholder; replace with per-circuit id if you obtain it from SignalR/circuit context.
		_circuitId = GetOrCreateCircuitId();
	}

	private string GetOrCreateCircuitId()
	{
		// For now we use a single default key so the same token is visible across scopes.
		// If you want per-circuit isolation, change this to use real circuit IDs.
		return "default";
	}

	public string? GetToken()
	{
		var hasToken = _tokens.TryGetValue(_circuitId, out var token);
		_logger.LogDebug("🔍 TokenProvider.GetToken (Circuit: {CircuitId}): HasToken={HasToken}",
			_circuitId, hasToken);
		return token;
	}

	public async Task SetTokenAsync(string token, ILocalStorageService localStorage)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new ArgumentException("Token cannot be null or empty", nameof(token));
		}

		_logger.LogInformation("💾 TokenProvider.SetToken (Circuit: {CircuitId}): Length={Length}",
			_circuitId, token.Length);

		// Store in static dictionary (shared across all instances)
		_tokens[_circuitId] = token;

		try
		{
			// Persist to localStorage for page refresh scenarios (localStorage is scoped and passed in)
			if (localStorage is not null)
			{
				await localStorage.SetItemAsync(TokenKey, token);
				_logger.LogInformation("✅ TokenProvider: Token stored in localStorage");
			}
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogDebug(ex, "⚠️ TokenProvider: Could not persist to localStorage (pre-rendering)");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ TokenProvider: Error persisting token");
		}
	}

	public async Task ClearTokenAsync(ILocalStorageService localStorage)
	{
		_logger.LogInformation("🗑️ TokenProvider.ClearToken (Circuit: {CircuitId})", _circuitId);

		// Remove from static dictionary
		_tokens.TryRemove(_circuitId, out _);

		try
		{
			if (localStorage is not null)
			{
				await localStorage.RemoveItemAsync(TokenKey);
				_logger.LogInformation("✅ TokenProvider: Token cleared from localStorage");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ TokenProvider: Error clearing token from localStorage");
		}
	}

	public async Task InitializeAsync(ILocalStorageService localStorage)
	{
		_logger.LogInformation("🔄 TokenProvider.Initialize (Circuit: {CircuitId})", _circuitId);

		// Check if we already have a token in static storage
		if (_tokens.ContainsKey(_circuitId))
		{
			_logger.LogInformation("✅ Token already exists in static storage");
			return;
		}

		try
		{
			// Try to load from localStorage (localStorage is scoped and passed in)
			if (localStorage is not null)
			{
				var token = await localStorage.GetItemAsync<string>(TokenKey);

				if (!string.IsNullOrWhiteSpace(token))
				{
					_tokens[_circuitId] = token;
					_logger.LogInformation("✅ Token loaded from localStorage and stored in static storage");
				}
			}
		}
		catch (InvalidOperationException)
		{
			_logger.LogDebug("⚠️ Could not load token during pre-rendering");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error loading token from localStorage");
		}
	}
}