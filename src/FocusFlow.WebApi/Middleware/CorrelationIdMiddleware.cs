using Serilog.Context;

namespace FocusFlow.WebApi.Middleware;

public class CorrelationIdMiddleware
{
	private readonly RequestDelegate _next;
	private const string CorrelationIdHeader = "X-Correlation-ID";
	private const string CorrelationIdKey = "CorrelationId";

	public CorrelationIdMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Get or generate correlation ID
		var correlationId = GetOrCreateCorrelationId(context);

		// Store in HttpContext.Items for easy access
		context.Items[CorrelationIdKey] = correlationId;

		// Add to response headers for client tracking
		context.Response.Headers[CorrelationIdHeader] = correlationId;

		// Enrich Serilog context for automatic logging
		using (LogContext.PushProperty(CorrelationIdKey, correlationId))
		{
			await _next(context);
		}
	}

	private static string GetOrCreateCorrelationId(HttpContext context)
	{
		// Check if client sent correlation ID
		if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) 
		    && !string.IsNullOrWhiteSpace(correlationId))
		{
			return correlationId.ToString();
		}

		// Generate new correlation ID
		return Guid.NewGuid().ToString();
	}
}
