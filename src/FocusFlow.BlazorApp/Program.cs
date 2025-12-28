using Blazored.LocalStorage;
using FluentValidation;
using Fluxor;
using FocusFlow.BlazorApp.Auth;
using FocusFlow.BlazorApp.Components;
using FocusFlow.BlazorApp.Features.Projects;
using FocusFlow.BlazorApp.Features.Dashboard;
using FocusFlow.BlazorApp.Features.Home;
using FocusFlow.BlazorApp.Features.Projects.Shared.Services;
using FocusFlow.BlazorApp.Features.Auth.Login.Validation;
using FocusFlow.BlazorApp.Features.Auth.Register.Validation;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using Refit;
using Serilog;
using System.Text;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Services;
using FocusFlow.BlazorApp.Features.Dashboard.Shared.Services;

// bootstrap logger to log events during startup.
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.BrowserConsole()
	// .WriteTo.Seq("http://focusflow-seq:5341")
	.CreateBootstrapLogger();

Log.Information("FocusFlow.BlazorApp starting up...");

try
{
	var builder = WebApplication.CreateBuilder(args);

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

	// Blazored.LocalStorage
	builder.Services.AddBlazoredLocalStorage();

	builder.Services.AddAuthorizationCore();

	// Custom AuthenticationStateProvider
	builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
	builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

	// HTTP Client + API service with Refit
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

	// FluentValidation
	builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

	// Fluxor
	builder.Services.AddFluxor(options =>
	{
		options.ScanAssemblies(typeof(Program).Assembly);
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

	app.UseAntiforgery();

	app.UseAuthentication(); 
	app.UseAuthorization();

	app.UseSerilogRequestLogging();

	app.MapRazorPages();
	app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

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