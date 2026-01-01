using FluentValidation;
using Fluxor;
using FocusFlow.BlazorApp.Auth;
using FocusFlow.BlazorApp.Components;
using FocusFlow.BlazorApp.Features.Auth.Login.Validation;
using FocusFlow.BlazorApp.Features.Auth.Register.Validation;
using FocusFlow.BlazorApp.Features.Dashboard;
using FocusFlow.BlazorApp.Features.Dashboard.Shared.Services;
using FocusFlow.BlazorApp.Features.Home;
using FocusFlow.BlazorApp.Features.Projects;
using FocusFlow.BlazorApp.Features.Projects.Shared.Services;
using FocusFlow.BlazorApp.Features.Tasks;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Services;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Services.Api;
using FocusFlow.BlazorApp.Shared.Services.SignalR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MudBlazor.Services;
using Refit;
using Serilog;
using System.Text;

// bootstrap logger to log events during startup.
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.BrowserConsole()
	.WriteTo.Seq("http://focusflow-seq:5341")
	.CreateBootstrapLogger();

Log.Information("FocusFlow.BlazorApp starting up...");

try
{
	var builder = WebApplication.CreateBuilder(args);

	// Configure graceful shutdown (give requests 15s to complete before forceful termination)
	builder.Services.Configure<HostOptions>(options =>
	{
		options.ShutdownTimeout = TimeSpan.FromSeconds(15);
	});

	// Replace the default logger with Serilog
	builder.Host.UseSerilog((context, configuration) => configuration
		.ReadFrom.Configuration(context.Configuration));

	// Configure Data Protection to persist keys to a shared location
	builder.Services.AddDataProtection()
		.PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"))
		.SetApplicationName("FocusFlow")
		.SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Keys expire after 90 days

	//// TODO: Optional: Add encryption at rest (production)
	//if (!builder.Environment.IsDevelopment())
	//{
	//	// You can add certificate-based encryption here
	//	// .ProtectKeysWithCertificate(...)
	//}

	// Add services to the container.
	builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

	// Add Razor Pages so _Host.cshtml tag helpers work
	builder.Services.AddRazorPages();

	// Add MudBlazor services
	builder.Services.AddMudServices();

	// ❌ REMOVE Blazored.LocalStorage - doesn't work with Blazor Server!
	// Use ProtectedBrowserStorage instead (built-in)
	// builder.Services.AddBlazoredLocalStorage();

	// Add minimal authentication scheme for Blazor Server
	// This is required to use [Authorize] attributes in Blazor Server
	builder.Services.AddAuthentication(options =>
	{
		// Set a default scheme (required by ASP.NET Core)
		options.DefaultScheme = "BlazorAuth";
		options.DefaultChallengeScheme = "BlazorAuth";
	})
	.AddScheme<AuthenticationSchemeOptions, BlazorAuthenticationHandler>("BlazorAuth", null);

	// Authorization (not Authentication - that's handled by JWT)
	builder.Services.AddAuthorizationCore();

	// Custom AuthenticationStateProvider for JWT
	builder.Services.AddScoped<ITokenProvider, TokenProvider>();
	builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

	// Register Refit HTTP clients and the API service (adds AuthHeaderHandler and base address).
	builder.Services.AddTransient<AuthHeaderHandler>();
	var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";
	
	// Register Refit clients
	builder.Services.AddRefitClient<IAuthApi>()
		.ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
		.AddHttpMessageHandler<AuthHeaderHandler>();

	builder.Services.AddRefitClient<IProjectsApi>()
		.ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
		.AddHttpMessageHandler<AuthHeaderHandler>();

	builder.Services.AddRefitClient<ITasksApi>()
		.ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
		.AddHttpMessageHandler<AuthHeaderHandler>();

	builder.Services.AddRefitClient<IDashboardApi>()
		.ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
		.AddHttpMessageHandler<AuthHeaderHandler>();

	// Register the main API service
	builder.Services.AddScoped<IApiService, RefitApiService>();

	// Register Features
	builder.Services.AddProjectsFeature();
	builder.Services.AddDashboardFeature();
	builder.Services.AddHomeFeature();
	builder.Services.AddTasksFeature();

	// SignalR Services - use Scoped so it can consume scoped services (e.g. ITokenProvider)
	builder.Services.AddScoped<ISignalRService, SignalRService>();
	builder.Services.AddScoped<SignalRTasksListener>();

	// FluentValidation
	builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

	// Fluxor
	builder.Services.AddFluxor(options =>
	{
		options.ScanAssemblies(typeof(Program).Assembly);
		// TODO: Add Redux DevTools when available in Fluxor version
	});

	// Health Checks for Kubernetes
	// Add HttpClient factory
	builder.Services.AddHttpClient();

	// Health Checks
	builder.Services.AddHealthChecks()
		.AddCheck("self", () => HealthCheckResult.Healthy("Blazor app is running"), tags: new[] { "live" })
		.AddCheck<ApiHealthCheck>("api", tags: new[] { "ready" });

	// Add response caching for health checks
	builder.Services.Configure<HealthCheckPublisherOptions>(options =>
	{
		options.Delay = TimeSpan.FromSeconds(5);
		options.Period = TimeSpan.FromSeconds(10);
	});

	var app = builder.Build();

	if (!app.Environment.IsDevelopment())
	{
		app.UseExceptionHandler("/Error", createScopeForErrors: true);
		app.UseHsts();
	}

	app.UseHttpsRedirection();
	// serving static files
	app.UseStaticFiles(); // TODO: in NET9, we use app.MapStaticAssets() instead // same thing but more efficiently: uses e-tags, caching, etc.

	// TODO: Remove UseAntiforgery() - not needed for API-first architecture with JWT authentication
	app.UseAntiforgery(); // CSRF protection handled by JWT tokens and Same-Origin Policy

	/*
	 * Because this app uses a custom AuthenticationStateProvider / token-based approach for Blazor Server UI, 
	 * and authorisation inside components is driven by that provider — 
	 * so the HTTP authentication middleware (UseAuthentication) was intentionally omitted. 
	 * 
	 * UseAuthorization is still needed for policy checks that run on requests or for [Authorize] on 
	 * RazorPages; Blazor component authorization uses the AuthenticationStateProvider instead.
	 */
	app.UseAuthorization();

	app.UseSerilogRequestLogging();

	app.MapRazorPages();
	app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

	// Health check endpoints for Kubernetes
	app.MapHealthChecks("/health");
	app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
	{
		Predicate = check => check.Tags.Contains("ready")
	});
	app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
	{
		Predicate = _ => false // Only self-checks for liveness
	});

	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "An unhandled exception occurred during BlazorApp bootstrapping");
}
finally
{
	Log.Information("BlazorApp shut down complete");
	await Log.CloseAndFlushAsync();
}