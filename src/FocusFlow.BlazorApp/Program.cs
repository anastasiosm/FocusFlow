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
using System.Text;

// bootstrap logger to log events during startup.
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.BrowserConsole()
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

	// Blazored.LocalStorage
	builder.Services.AddBlazoredLocalStorage();

	builder.Services.AddAuthorizationCore();

	// Custom AuthenticationStateProvider
	builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
	builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

	// HTTP Client + API service
	builder.Services.AddTransient<AuthHeaderHandler>();

	var useFakeApi = builder.Configuration.GetValue<bool>("UseFakeApi");
	if (useFakeApi)
	{
		builder.Services.AddSingleton<IApiService, FakeApiService>();
		builder.Services.AddHttpClient();
	}
	else
	{
		var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";
		builder.Services.AddHttpClient("FocusFlowAPI", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		}).AddHttpMessageHandler<AuthHeaderHandler>();

		builder.Services.AddScoped<IApiService>(sp =>
		{
			var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("FocusFlowAPI");
			var logger = sp.GetRequiredService<ILogger<ApiService>>();
			return new ApiService(httpClient, logger);
		});
	}

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
	app.UseStaticFiles();

	app.UseAntiforgery();

	app.UseAuthentication(); // ✅ must come BEFORE UseAuthorization
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