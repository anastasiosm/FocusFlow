using FocusFlow.WebApi;
using FocusFlow.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .ConfigureServices()
    .ConfigurePipelineAsync();

if (app.Environment.IsDevelopment())
{
	await app.Services.ApplyMigrationsAsync();
}

app.Run();