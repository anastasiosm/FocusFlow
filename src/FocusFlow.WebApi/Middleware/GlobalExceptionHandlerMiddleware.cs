using FocusFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FocusFlow.WebApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;
	private readonly IWebHostEnvironment _env;

	public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
	{
		_logger = logger;
		_env = env;
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		// 1. Map exception to status code
		var statusCode = exception switch
		{
			FocusFlowNotFoundException => HttpStatusCode.NotFound,
			FocusFlowValidationException or FocusFlowBusinessRuleException or InvalidOperationException => HttpStatusCode.BadRequest,
			FocusFlowUnauthorizedException => HttpStatusCode.Unauthorized,
			_ => HttpStatusCode.InternalServerError
		};

		// Get correlation ID
		var correlationId = httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier;

		// 2. Structured Logging
		_logger.LogError(exception,
			"[{ExceptionType}] [CorrelationId: {CorrelationId}] An unhandled exception occurred: {Message}. TraceId: {TraceId}, Path: {Path}",
			exception.GetType().Name, correlationId, exception.Message, httpContext.TraceIdentifier, httpContext.Request.Path);

		// 3. Build ProblemDetails (RFC 7807)
		var problemDetails = new ProblemDetails
		{
			Status = (int)statusCode,
			Title = GetTitle(exception, statusCode),
			Detail = exception.Message,
			Instance = httpContext.Request.Path,
			Extensions = new Dictionary<string, object?>
			{
				["traceId"] = httpContext.TraceIdentifier,
				["correlationId"] = correlationId
			}
		};

		// Validation errors
		if (exception is FocusFlowValidationException valEx)
		{
			problemDetails.Extensions["errors"] = valEx.Errors;
		}

		// For unhandled server errors, do not expose details in non-development environments
		if (statusCode == HttpStatusCode.InternalServerError && !_env.IsDevelopment())
		{
			problemDetails.Detail = "An error occurred while processing your request.";
		}

		// Include stack trace only in Development
		if (_env.IsDevelopment())
		{
			problemDetails.Extensions["stackTrace"] = exception.StackTrace;
		}

		httpContext.Response.StatusCode = problemDetails.Status.Value;

		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}

	private static string GetTitle(Exception exception, HttpStatusCode statusCode) => exception switch
	{
		FocusFlowNotFoundException => "Resource Not Found",
		FocusFlowValidationException => "Validation Failed",
		FocusFlowBusinessRuleException => "Business Rule Violation",
		FocusFlowUnauthorizedException => "Unauthorized",
		InvalidOperationException => "Invalid Operation",
		_ => statusCode == HttpStatusCode.InternalServerError ? "Internal Server Error" : "An error occurred"
	};
}
