using Fluxor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FocusFlow.BlazorApp.Auth;

/// <summary>
/// Minimal authentication handler for Blazor Server.
/// Delegates actual authentication to CustomAuthenticationStateProvider.
/// Only exists to satisfy ASP.NET Core authentication middleware requirements.
/// </summary>
public class BlazorAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthenticationStateProvider authStateProvider)
        : base(options, logger, encoder)
    {
        _authStateProvider = authStateProvider;
	}

	/// <summary>
	/// Its job is to bridge Blazor's AuthenticationStateProvider (Blazor Server auth model) into ASP.NET Core's authentication pipeline 
    /// by returning an AuthenticateResult the middleware understands.
    /// 
    /// Why this exists?
    /// Blazor Server keeps auth state in a Blazor-specific provider(AuthenticationStateProvider). 
    /// ASP.NET Core middleware expects an AuthenticationHandler to participate in authentication.
    /// This handler simply delegates to the Blazor provider so middleware and attributes like[Authorize] work as expected.
	/// </summary>    
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
			// Get authentication state from our custom provider:

			// This provider knows how to get the current user in Blazor Server context
			var authState = await _authStateProvider.GetAuthenticationStateAsync();

			// Extract user principal from state
			var user = authState.User;

            // If user is authenticated, return success
            if (user?.Identity?.IsAuthenticated == true)
            {
				// Create a ticket AuthenticationTicket (ASP.NET Core auth system) with user principal 
				// (for [Authorize] etc) 
				var ticket = new AuthenticationTicket(user, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            // Not authenticated
            return AuthenticateResult.NoResult();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in BlazorAuthenticationHandler");
            return AuthenticateResult.Fail(ex);
        }
    }

    // Challenge = redirect to login (for [Authorize] attributes)
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Redirect to login page
        Response.Redirect("/login");
        return Task.CompletedTask;
    }
}