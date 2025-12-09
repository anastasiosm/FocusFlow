using FocusFlow.WebApi;
using FocusFlow.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .ConfigureServices()
    .ConfigurePipelineAsync();

if (app.Environment.IsDevelopment())
{
	// Force reset (DELETES DB) — use only in dev
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<FocusFlow.Infrastructure.Data.FocusFlowDbContext>();	
	await db.Database.EnsureDeletedAsync();

	await app.Services.ApplyMigrationsAsync();
	await app.Services.SeedAsync();
}

app.Run();