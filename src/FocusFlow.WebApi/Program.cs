using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Data.Extensions;
using FocusFlow.WebApi;
using Serilog;

// Use a bootstrap logger to log events during startup.
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.Seq("http://focusflow-seq:5341")
	.CreateBootstrapLogger();

Log.Information("FocusFlow.WebApi starting up...");

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

	// Configure services and pipeline
	builder.ConfigureServices();
	var app = builder.Build();
	app.ConfigurePipelineAsync();

	// Seed the database in development environment
	if (app.Environment.IsDevelopment())
	{
		using var scope = app.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<FocusFlowDbContext>();
		await db.Database.EnsureDeletedAsync(); // For development, start with a clean slate

		await app.Services.ApplyMigrationsAsync();
		await app.Services.SeedAsync();
	}

	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
	Log.Information("Shut down complete");
	await Log.CloseAndFlushAsync();
}


public partial class Program { }
