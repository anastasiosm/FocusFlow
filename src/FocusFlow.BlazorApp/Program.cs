using FocusFlow.BlazorApp.Components;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Auth; 
using MudBlazor.Services;
using Blazored.LocalStorage; 
using Microsoft.AspNetCore.Components.Authorization; 
using FluentValidation; 
using FocusFlow.BlazorApp.Models.Validators; 
using Fluxor;

var builder = WebApplication.CreateBuilder(args);

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

// Map Razor Pages so _Host.cshtml is served
app.MapRazorPages();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

await app.RunAsync();