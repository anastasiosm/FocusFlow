using FocusFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FocusFlow.WebApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;
	private readonly IWebHostEnvironment _env;

	public GlobalExceptionHandler(
		ILogger<GlobalExceptionHandler> logger,
		IWebHostEnvironment env)
	{
		_logger = logger;
		_env = env;
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var errors = exception is FocusFlowValidationException validationEx ? validationEx.Errors : null;
		
		_logger.LogError(exception,
			"Exception occurred: {Message} {@Errors} {@Exception}",
			exception.Message,
			errors,
			new { 
				Type = exception.GetType().Name,
				Path = httpContext.Request.Path.Value,
				Method = httpContext.Request.Method,
				TraceId = httpContext.TraceIdentifier,
				StatusCode = GetStatusCodeAndMessage(exception).statusCode
			});

		var (statusCode, message) = GetStatusCodeAndMessage(exception);

		httpContext.Response.StatusCode = (int)statusCode;
		httpContext.Response.ContentType = "application/json";

		var response = new Dictionary<string, object?>
		{
			["error"] = message,
			["statusCode"] = (int)statusCode,
			["traceId"] = httpContext.TraceIdentifier,
			["path"] = httpContext.Request.Path.Value
		};

		if (exception is FocusFlowValidationException valEx)
		{
			response["errors"] = valEx.Errors;
		}
		else if (_env.IsDevelopment())
		{
			response["stackTrace"] = exception.StackTrace;
		}

		await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

		return true; // Exception handled
	}

	private static (HttpStatusCode statusCode, string message) GetStatusCodeAndMessage(Exception exception)
	{
		return exception switch
		{
			FocusFlowNotFoundException =>
				(HttpStatusCode.NotFound, exception.Message),

			FocusFlowValidationException =>
				(HttpStatusCode.BadRequest, exception.Message),

			FocusFlowBusinessRuleException =>
				(HttpStatusCode.BadRequest, exception.Message),

			UnauthorizedAccessException =>
				(HttpStatusCode.Forbidden, exception.Message),

			InvalidOperationException =>
				(HttpStatusCode.BadRequest, exception.Message),

			_ => (HttpStatusCode.InternalServerError,
				  "An error occurred while processing your request.")
		};
	}
}
