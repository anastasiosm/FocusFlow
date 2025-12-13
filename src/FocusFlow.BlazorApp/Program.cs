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
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthHeaderHandler>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>(); 

// Add Fluxor
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly); 
    // options.UseReduxDevTools(rdt => { rdt.Name = "FocusFlow Blazor App"; }); // Optional: For Fluxor DevTools integration - commented due to build error
});

// Add API service registration
var useFakeApi = builder.Configuration.GetValue<bool>("UseFakeApi");

if (useFakeApi)
{
    // Use the fake API service for local development without a running backend
    builder.Services.AddSingleton<IApiService, FakeApiService>();
    // Also add HttpClient for authentication components
    builder.Services.AddHttpClient();
}
else
{
    // Use the real API service
    builder.Services.AddHttpClient<IApiService, ApiService>(client =>
    {
        var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();
}

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