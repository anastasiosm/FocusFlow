using FluentAssertions;
using FocusFlow.Domain.Exceptions;
using FocusFlow.WebApi.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace FocusFlow.WebApi.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
	private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
	private readonly Mock<IWebHostEnvironment> _mockEnv;
	private readonly GlobalExceptionHandler _handler;

	public GlobalExceptionHandlerTests()
	{
		_mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
		_mockEnv = new Mock<IWebHostEnvironment>();
		
		// Default to Production environment
		_mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

		_handler = new GlobalExceptionHandler(_mockLogger.Object, _mockEnv.Object);
	}

	[Fact]
	public async Task TryHandleAsync_WithNotFoundException_ShouldReturn404()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new FocusFlowNotFoundException("Entity", Guid.NewGuid());

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
		httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("detail").GetString().Should().Contain("Entity");
		response.GetProperty("status").GetInt32().Should().Be(404);
		response.GetProperty("title").GetString().Should().Be("Resource Not Found");
	}

	[Fact]
	public async Task TryHandleAsync_WithValidationException_ShouldReturn400()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var errors = new Dictionary<string, string[]>
		{
			{ "Title", new[] { "Title is required" } }
		};
		var exception = new FocusFlowValidationException(errors);

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
		httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("status").GetInt32().Should().Be(400);
		response.GetProperty("title").GetString().Should().Be("Validation Failed");
		// errors are included in extensions
		response.GetProperty("errors").TryGetProperty("Title", out _).Should().BeTrue();
	}

	[Fact]
	public async Task TryHandleAsync_WithBusinessRuleException_ShouldReturn400()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new FocusFlowBusinessRuleException("Business rule violated");

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
		httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("detail").GetString().Should().Be("Business rule violated");
		response.GetProperty("title").GetString().Should().Be("Business Rule Violation");
	}

	[Fact]
	public async Task TryHandleAsync_WithUnauthorizedAccessException_ShouldReturn403()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new FocusFlowUnauthorizedException("Access denied");

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
		httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("detail").GetString().Should().Be("Access denied");
		response.GetProperty("status").GetInt32().Should().Be(403);
		response.GetProperty("title").GetString().Should().Be("Forbidden");
	}

	[Fact]
	public async Task TryHandleAsync_WithInvalidOperationException_ShouldReturn400()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new InvalidOperationException("Invalid operation");

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
		
		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("title").GetString().Should().Be("Invalid Operation");
	}

	[Fact]
	public async Task TryHandleAsync_WithUnhandledException_ShouldReturn500()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new Exception("Something went wrong");

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
		httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("detail").GetString().Should().Be("An error occurred while processing your request.");
		response.GetProperty("status").GetInt32().Should().Be(500);
		response.GetProperty("title").GetString().Should().Be("Internal Server Error");
	}

	[Fact]
	public async Task TryHandleAsync_InProduction_ShouldNotIncludeStackTrace()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();

		var exception = new Exception("Test exception");

		// Act
		var result = await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		result.Should().BeTrue();

		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.TryGetProperty("stackTrace", out var stackTrace).Should().BeFalse();
	}

	[Fact]
	public async Task TryHandleAsync_ShouldLogError()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();
		httpContext.Request.Path = "/api/test";
		httpContext.Request.Method = "GET";

		var exception = new Exception("Test exception");

		// Act
		await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		_mockLogger.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				exception,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	[Fact]
	public async Task TryHandleAsync_ShouldIncludeTraceId()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();
		httpContext.TraceIdentifier = "test-trace-id";

		var exception = new Exception("Test exception");

		// Act
		await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("traceId").GetString().Should().Be("test-trace-id");
	}

	[Fact]
	public async Task TryHandleAsync_ShouldIncludeCorrelationId()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();
		httpContext.Items["CorrelationId"] = "test-correlation-id";

		var exception = new Exception("Test exception");

		// Act
		await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("correlationId").GetString().Should().Be("test-correlation-id");
	}

	[Fact]
	public async Task TryHandleAsync_ShouldIncludeInstancePath()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		httpContext.Response.Body = new MemoryStream();
		httpContext.Request.Path = "/api/projects/123";

		var exception = new Exception("Test exception");

		// Act
		await _handler.TryHandleAsync(
			httpContext,
			exception,
			CancellationToken.None);

		// Assert
		httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(httpContext.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
		response.GetProperty("instance").GetString().Should().Be("/api/projects/123");
	}
}
