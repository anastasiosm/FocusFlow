using Blazored.LocalStorage; 
using FluentValidation; 
using Fluxor;
using FocusFlow.BlazorApp.Auth; 
using FocusFlow.BlazorApp.Components;
using FocusFlow.BlazorApp.Models.Validators; 
using FocusFlow.BlazorApp.Services;
using Microsoft.AspNetCore.Components.Authorization; 
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using Serilog;

// Use a bootstrap logger to log events during startup.
// For Blazor, this will log to the server console during startup, 
// and the BrowserConsole sink will take over for client-side events.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.BrowserConsole()    // optional: sends logs to browser console for Blazor Server
    .CreateBootstrapLogger();

Log.Information("FocusFlow.BlazorApp starting up...");

try
{
	var builder = WebApplication.CreateBuilder(args);

	// Replace the default logger with Serilog
	builder.Host.UseSerilog((context, services, configuration) => configuration
		.ReadFrom.Configuration(context.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext());

	// Configure Data Protection to persist keys to a shared location
	builder.Services.AddDataProtection()
		.PersistKeysToFileSystem(new DirectoryInfo("/dataprotection-keys"))
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

	// Add Auth services
	builder.Services.AddBlazoredLocalStorage();

	// ✅ CRITICAL FIX: Singleton so the SAME instance is shared across ALL scopes
	// This ensures AuthHeaderHandler (transient) and AuthEffects (scoped) see the SAME token
	// ILocalStorageService is passed as parameter (can't inject Scoped into Singleton)
	builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
	builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
	builder.Services.AddAuthorizationCore();

	// Transient - New instance for each request
	builder.Services.AddTransient<AuthHeaderHandler>();

	// Add FluentValidation
	builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

	// Add Fluxor
	builder.Services.AddFluxor(options =>
	{
		options.ScanAssemblies(typeof(Program).Assembly);
	});

	// Add API service registration
	var useFakeApi = builder.Configuration.GetValue<bool>("UseFakeApi");

	if (useFakeApi)
	{
		// Use the fake API service for local development without a running backend
		builder.Services.AddSingleton<IApiService, FakeApiService>();
		builder.Services.AddHttpClient();
	}
	else
	{
		// ✅ ΣΩΣΤΗ ΛΥΣΗ: Named HttpClient με handler
		var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";

		// 1. Register named HttpClient with AuthHeaderHandler
		builder.Services.AddHttpClient("FocusFlowAPI", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		})
		.AddHttpMessageHandler<AuthHeaderHandler>();

		// 2. Register IApiService that uses the named HttpClient
		builder.Services.AddScoped<IApiService>(sp =>
		{
			var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("FocusFlowAPI");
			var logger = sp.GetRequiredService<ILogger<ApiService>>();
			return new ApiService(httpClient, logger);
		});
	}

	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (!app.Environment.IsDevelopment())
	{
		app.UseExceptionHandler("/Error", createScopeForErrors: true);
		app.UseHsts();
	}

	app.UseHttpsRedirection();

	// Serve static files from wwwroot AND _content (for Razor Class Libraries like MudBlazor)
	app.UseStaticFiles();

	app.UseAntiforgery();
    
    // This is important for Serilog to work correctly with request-scoped services
    app.UseSerilogRequestLogging();

	// Map Razor Pages so _Host.cshtml is served
	app.MapRazorPages();

	app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

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