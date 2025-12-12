using FocusFlow.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace FocusFlow.WebApi.Middleware;

public class GlobalExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

	public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An unhandled exception occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		var statusCode = HttpStatusCode.InternalServerError;
		var message = "An error occurred while processing your request.";

		switch (exception)
		{
			case FocusFlowNotFoundException:
				statusCode = HttpStatusCode.NotFound;
				message = exception.Message;
				break;
			case FocusFlowValidationException:
				statusCode = HttpStatusCode.BadRequest;
				message = exception.Message;
				break;
			case FocusFlowBusinessRuleException:
				statusCode = HttpStatusCode.BadRequest;
				message = exception.Message;
				break;
			case UnauthorizedAccessException:
				statusCode = HttpStatusCode.Forbidden;
				message = exception.Message;
				break;
			case InvalidOperationException:
				statusCode = HttpStatusCode.BadRequest;
				message = exception.Message;
				break;
		}

		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		var response = new
		{
			error = message,
			statusCode = (int)statusCode
		};

		return context.Response.WriteAsync(JsonSerializer.Serialize(response));
	}
}
