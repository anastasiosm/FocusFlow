using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Bunit.TestDoubles;

namespace FocusFlow.BlazorApp.Tests;

/// <summary>
/// Base class for bUnit component tests.
/// Provides common setup for services required by components under test.
/// </summary>
public abstract class TestContextBase : TestContext
{
    protected TestAuthorizationContext AuthContext { get; }

    protected TestContextBase()
    {
        // Add MudBlazor services (required for components that use MudBlazor)
        Services.AddMudServices();
        
        // Add TestAuthorizationContext and set a default authorized user
        AuthContext = this.AddTestAuthorization();
        AuthContext.SetAuthorized("testuser");

        // Setup common JSInterop calls for MudBlazor components
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        JSInterop.SetupVoid("mudOverlay.unlockScroll", _ => true);
        JSInterop.SetupVoid("mudOverlay.lockScroll", _ => true);
        JSInterop.SetupVoid("mudScrollManager.unlockScroll", _ => true);
        JSInterop.SetupVoid("mudScrollManager.lockScroll", _ => true);

        // Setup any other common services required by tests
        // e.g. logging, configuration, etc.
    }

    /// <summary>
    /// Helper method for generating test GUIDs in a consistent format
    /// </summary>
    protected static Guid CreateTestGuid(int seed = 1)
    {
        return new Guid($"00000000-0000-0000-0000-{seed:D12}");
    }
}
