using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Collections.Concurrent;

namespace FocusFlow.BlazorApp.Services;

/// <summary>
/// Provides methods to get, set, and clear authentication tokens using ProtectedLocalStorage with static caching. 
/// </summary>
public class TokenProvider : ITokenProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly ILogger<TokenProvider> _logger;
    	
	// Static cache uses a static ConcurrentDictionary to share state across all instances.
	private static readonly ConcurrentDictionary<string, string?> _tokenCache = new();
    private static readonly object _initLock = new();
    private static bool _initialized = false;
    
    private const string TOKEN_KEY = "focusflow_auth_token";
    private const string CACHE_KEY = "current_token";

    public TokenProvider(
        ProtectedLocalStorage protectedLocalStorage,
        ILogger<TokenProvider> logger)
    {
        _protectedLocalStorage = protectedLocalStorage;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously retrieves the current authentication token from the cache or underlying storage.
    /// </summary>
    /// <remarks>If the token is not present in the cache, the method attempts to initialize and retrieve it
    /// from persistent storage. Subsequent calls may return a cached value for improved performance.</remarks>
    /// <returns>A string containing the authentication token if available; otherwise, null.</returns>
    public async Task<string?> GetTokenAsync()
    {
        // Try to get from static cache first
        if (_tokenCache.TryGetValue(CACHE_KEY, out var cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }

        // Initialize from storage if not already done
        if (!_initialized)
        {
            await InitializeAsync();
        }

        // Return cached token after initialization
        _tokenCache.TryGetValue(CACHE_KEY, out var token);
        return token;
    }

    /// <summary>
    /// Asynchronously sets or removes the authentication token in both memory and protected local storage.
    /// </summary>
    /// <remarks>This method updates an in-memory cache immediately and persists the token to protected local
    /// storage. If the token is removed, any previously stored token is deleted. The method is thread-safe and can be
    /// called multiple times. Logging is performed for both successful and failed storage operations.</remarks>
    /// <param name="token">The authentication token to store. If null or empty, the existing token is removed from storage.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetTokenAsync(string? token)
    {
        // Update static cache immediately
        if (string.IsNullOrEmpty(token))
        {
            _tokenCache.TryRemove(CACHE_KEY, out _);
        }
        else
        {
            _tokenCache[CACHE_KEY] = token;
        }

        // Persist to storage
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                await _protectedLocalStorage.DeleteAsync(TOKEN_KEY);
                _logger.LogInformation("🗑️ Token removed from storage");
            }
            else
            {
                await _protectedLocalStorage.SetAsync(TOKEN_KEY, token);
                _logger.LogInformation("💾 Token saved to protected storage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to save token to storage");
        }

        lock (_initLock)
        {
            _initialized = true;
        }
    }

    /// <summary>
    /// Asynchronously removes the currently stored authentication token, if any.   
    /// </summary>
    /// <remarks>After calling this method, subsequent operations that require authentication may fail until a
    /// new token is set.</remarks>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    public async Task ClearTokenAsync()
    {
        await SetTokenAsync(null);
    }
    
    /// <summary>
    /// Lazy initialization: loads token from ProtectedLocalStorage into static cache.
    /// Called automatically by GetTokenAsync() on first access. Thread-safe with locks.
    /// </summary>
    private async Task InitializeAsync()
    {
        lock (_initLock)
        {
            if (_initialized) return;
        }

        try
        {
            var result = await _protectedLocalStorage.GetAsync<string>(TOKEN_KEY);
            
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _tokenCache[CACHE_KEY] = result.Value;
                _logger.LogInformation("✅ Token loaded from protected storage");
            }
            else
            {
                _logger.LogInformation("ℹ️ No token found in storage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to load token from storage");
        }
        finally
        {
            lock (_initLock)
            {
                _initialized = true;
            }
        }
    }

    // Backward compatibility methods
    public string? GetToken()
    {
        _tokenCache.TryGetValue(CACHE_KEY, out var token);
        return token;
    }

    public void SetToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _tokenCache.TryRemove(CACHE_KEY, out _);
        }
        else
        {
            _tokenCache[CACHE_KEY] = token;
        }
        // Note: This doesn't persist to storage - use SetTokenAsync for persistence
    }

    public void ClearToken()
    {
        _tokenCache.TryRemove(CACHE_KEY, out _);
        // Note: This doesn't clear from storage - use ClearTokenAsync for persistence
    }
}