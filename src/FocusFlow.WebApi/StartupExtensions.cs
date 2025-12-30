using FocusFlow.Application;
using FocusFlow.Infrastructure;
using FocusFlow.Infrastructure.Identity;
using FocusFlow.WebApi.Authorization.ProjectOwnership;
using FocusFlow.WebApi.Authorization.TaskOwnership;
using FocusFlow.WebApi.Hubs;
using FocusFlow.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

namespace FocusFlow.WebApi;

public static class StartupExtensions
{
	// Configure services
	public static WebApplicationBuilder ConfigureServices
		(this WebApplicationBuilder builder)
	{
		builder.Services.AddApplicationServices();
		builder.Services.AddInfrastructure(builder.Configuration);
		
		// Configure Data Protection to use shared keys with Blazor app
		builder.Services.AddDataProtection()
			.PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"))
			.SetApplicationName("FocusFlow")
			.SetDefaultKeyLifetime(TimeSpan.FromDays(90));
		
		// Configure Identity specifics for WebAPI (SignInManager)
		builder.Services.AddIdentityCore<ApplicationUser>()
			.AddSignInManager<SignInManager<ApplicationUser>>();

		var jwtSettings = builder.Configuration.GetSection("JwtSettings");
		var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");

		// PRODUCTION-READY JWT AUTHENTICATION
		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = jwtSettings["Issuer"],
				ValidAudience = jwtSettings["Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
			};

			// Configure JWT for SignalR
			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					var accessToken = context.Request.Query["access_token"];

					// If the request is for our hub...
					var path = context.HttpContext.Request.Path;
					if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
					{
						// Read the token out of the query string
						context.Token = accessToken;
					}
					return Task.CompletedTask;
				}
			};
		});

		// Authorization policies
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddAuthorization(options =>
		{
			options.AddPolicy("ProjectOwner", policy =>
				policy.Requirements.Add(new ProjectOwnershipRequirement()));

			options.AddPolicy("TaskOwner", policy =>
				policy.Requirements.Add(new TaskOwnershipRequirement()));
		});
		builder.Services.AddScoped<IAuthorizationHandler, ProjectOwnershipHandler>();
		builder.Services.AddScoped<IAuthorizationHandler, TaskOwnershipHandler>();

		builder.Services.AddControllers()
			// We added AddJsonOptions with JsonStringEnumConverter to make enum values be sent and received as their names (strings) in JSON instead of numeric values.
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
			});

		// OpenAPI / Swagger
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FocusFlow API", Version = "v1" });

			// Include XML comments when available
			try
			{
				var xmlFile = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name + ".xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile ?? string.Empty);
				if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
			}
			catch { /* ignore */ }

			// JWT bearer security (if used)
			c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = Microsoft.OpenApi.Models.ParameterLocation.Header,
				Description = "JWT Authorization header using the Bearer scheme."
			});

			c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
				{ new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[]{} }
			});
		});

		builder.Services.AddCors(
			options => options.AddPolicy(
				"open",
				policy => policy.WithOrigins([
					builder.Configuration["ApiUrl"] ?? "http://localhost:8080",
					builder.Configuration["BlazorUrl"] ?? "http://localhost:5050",
					"https://localhost:5051"  // Add HTTPS Blazor URL
				])
				.AllowAnyMethod()
				.SetIsOriginAllowed(pol => true) // this setting is to allow subdomains
				.AllowAnyHeader()
				.AllowCredentials())); // Required for SignalR

		// Register SignalR services
		builder.Services.AddSignalR(options =>
		{
			options.EnableDetailedErrors = true;  // For debugging
			options.KeepAliveInterval = TimeSpan.FromSeconds(15);
			options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
		});

		// Register hub context mapping for dependency injection
		builder.Services.AddSingleton<IHubContext<Hub>>(provider =>
			provider.GetRequiredService<IHubContext<TasksHub>>());

		// Exception handling
		builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
		builder.Services.AddProblemDetails(); // standardized responses

		return builder;
	}

	// Configure the HTTP request pipeline.
	public static WebApplication ConfigurePipelineAsync(this WebApplication app)
	{
		// Global exception handler (must be first)
		app.UseExceptionHandler(); // Αυτό καλεί το registered IExceptionHandler

		// Enable endpoint routing before CORS / Authentication / Authorization
		app.UseRouting();

		app.UseCors("open");

		if (app.Environment.IsDevelopment())
		{
			// Swagger/OpenAPI (JSON generation) - published at /openapi/v1.json for Scalar
			app.UseSwagger(options =>
			{
				options.RouteTemplate = "openapi/{documentName}.json";
			});

			app.MapScalarApiReference(options =>
			{
				options
					.WithTitle("FocusFlow API")
					.WithTheme(ScalarTheme.Purple)
					.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
			});

			// Swagger UI for interactive API documentation (available at /swagger)
			app.UseSwaggerUI(c =>
			{
				c.RoutePrefix = "swagger";
				c.SwaggerEndpoint("/openapi/v1.json", "FocusFlow API v1");
				c.DocumentTitle = "FocusFlow API - Swagger";
			});
		}

		app.UseAuthentication();
		app.UseAuthorization();
		
		// Serilog request logging
		app.UseSerilogRequestLogging();
				
		//app.UseHttpsRedirection();
		app.MapControllers();

		// Map SignalR hub endpoint
		app.MapHub<TasksHub>("/hubs/tasks");

		return app;
	}	
}
