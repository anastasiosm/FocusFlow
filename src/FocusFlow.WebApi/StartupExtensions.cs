using FocusFlow.Application;
using FocusFlow.Infrastructure;

namespace FocusFlow.WebApi;

public static class StartupExtensions
{
	// Configure services
	public static WebApplication ConfigureServices
		(this WebApplicationBuilder builder)
	{
		builder.Services.AddApplicationServices();
		builder.Services.AddInfrastructure(builder.Configuration);

		builder.Services.AddControllers();

		builder.Services.AddCors(
			options => options.AddPolicy(
				"open",
				policy => policy.WithOrigins([builder.Configuration["ApiUrl"] ??
				"http://localhost:3000",
					builder.Configuration["BlazorUrl"] ??
					"http://localhost:5000"])
				.AllowAnyMethod()
				.SetIsOriginAllowed(pol => true) // this setting is to allow subdomains
				.AllowAnyHeader()
				.AllowCredentials()));

		return builder.Build();
	}

	// Configure the HTTP request pipeline.
	public static WebApplication ConfigurePipelineAsync(this WebApplication app)
	{
		app.UseCors("open");
		app.UseHttpsRedirection();
		app.MapControllers();

		return app;
	}	
}
