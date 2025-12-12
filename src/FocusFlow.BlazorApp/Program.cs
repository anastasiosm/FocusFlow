using FocusFlow.BlazorApp.Components;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Auth; // Added
using MudBlazor.Services;
using Blazored.LocalStorage; // Added
using Microsoft.AspNetCore.Components.Authorization; // Added

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
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthHeaderHandler>();

// Add HttpClient for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    // Configure base address from appsettings or use default
    var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

// IMPORTANT: Serve static files from wwwroot AND _content (for Razor Class Libraries like MudBlazor)
app.UseStaticFiles();

app.UseAntiforgery();

// Map Razor Pages so _Host.cshtml is served
app.MapRazorPages();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();