using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Data.Extensions;
using FocusFlow.WebApi;
using Serilog;
using Serilog.Events;

// Use a bootstrap logger to log events during startup.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("FocusFlow.WebApi starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace the default logger with Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        // Read configuration from appsettings.json as the primary source
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
		// Add any additional configuration from code that is not in appsettings
		.MinimumLevel.Information()
	    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
	    .MinimumLevel.Override("System", LogEventLevel.Warning)
	    .Enrich.FromLogContext()
		.Enrich.WithEnvironmentName()
		.Enrich.WithMachineName()
	    .Enrich.WithThreadId()
	    .WriteTo.Console(
		    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
	    .WriteTo.File(
		    path: "logs/focusflow-.log",
		    rollingInterval: RollingInterval.Day,
		    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
		    retainedFileCountLimit: 30));

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
