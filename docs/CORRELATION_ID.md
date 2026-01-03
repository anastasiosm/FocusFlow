# Correlation ID Implementation

## Overview

Correlation ID is implemented across the FocusFlow application to enable request tracing across all layers while maintaining Clean Architecture principles.

## Architecture Flow

```
+-------------------------------------+
|   CorrelationIdMiddleware           |  <- Generate/Extract ID
|   • Reads X-Correlation-ID header   |
|   • Generates new GUID if not present
|   • Stores in HttpContext.Items     |
|   • Enriches Serilog LogContext     |
|   • Adds to response headers        |
+------------------+------------------+
                   |
                   v
+-------------------------------------+
|   Controllers                       |  <- Extract & Pass to Commands
|   • Extract from HttpContext.Items  |
|   • Pass as command property        |
|   • Log with correlation ID         |
+------------------+------------------+
                   |
                   v
+-------------------------------------+
|   Command/Query Handlers            |  <- Use from Command Property
|   • Access via command.CorrelationId|
|   • Log business events with ID     |
|   • No HttpContext dependency       |
+------------------+------------------+
                   |
                   v
+-------------------------------------+
|   GlobalExceptionHandler            |  <- Include in Error Response
|   • Extract from HttpContext.Items  |
|   • Include in ProblemDetails       |
|   • Log with correlation ID         |
+-------------------------------------+
```

## Implementation Details

### 1. Middleware

**File**: `src/FocusFlow.WebApi/Middleware/CorrelationIdMiddleware.cs`

- Runs **early** in the pipeline (before exception handler)
- Checks for `X-Correlation-ID` header from client
- Generates new GUID if not present
- Stores in `HttpContext.Items["CorrelationId"]`
- Adds to response headers for client tracking
- Enriches Serilog `LogContext` for automatic logging

### 2. Command Properties

All commands/queries include optional `CorrelationId` property:

```csharp
public record LoginCommand(
    string Email,
    string Password,
    string? CorrelationId = null  // <- Metadata property
) : IRequest<AuthResponse>;
```

**Benefits**:
- ? Clean Architecture compliant (no HTTP coupling)
- ? Handlers remain HTTP-agnostic
- ? Easy unit testing
- ? Can run from background jobs

### 3. Controllers

Controllers extract correlation ID and pass to commands:

```csharp
[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
{
    var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

    var command = new LoginCommand(
        request.Email,
        request.Password,
        correlationId  // <- Pass to command
    );

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### 4. Logging

Correlation ID is automatically included in all logs via Serilog enrichment:

```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()  // <- Enable LogContext enrichment
    .Enrich.WithProperty("ApplicationName", "FocusFlow.WebApi")
    .Enrich.WithMachineName());
```

**Log output example**:
```
[10:15:23 INF] [CorrelationId: abc-123] Login attempt for email: user@example.com
[10:15:23 DBG] [CorrelationId: abc-123] Executing LoginCommand for user@example.com
[10:15:24 ERR] [CorrelationId: abc-123] [FocusFlowUnauthorizedException] Invalid password
```

### 5. Error Responses

Correlation ID is included in ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Forbidden",
  "status": 403,
  "detail": "Invalid email or password",
  "instance": "/api/auth/login",
  "traceId": "0HN7GQ8F2M3J4:00000001",
  "correlationId": "abc-123-def-456"
}
```

## Usage Examples

### Client sends Correlation ID (idempotent retry)

**Request**:
```http
POST /api/auth/login HTTP/1.1
X-Correlation-ID: client-generated-uuid
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response**:
```http
HTTP/1.1 200 OK
X-Correlation-ID: client-generated-uuid
Content-Type: application/json

{
  "token": "eyJhbGc...",
  ...
}
```

### Client doesn't send Correlation ID

**Request**:
```http
POST /api/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response**:
```http
HTTP/1.1 200 OK
X-Correlation-ID: a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d  <- Server-generated
Content-Type: application/json

{
  "token": "eyJhbGc...",
  ...
}
```

### Filtering logs in Seq

```sql
-- All logs for a specific request
CorrelationId = 'abc-123-def-456'

-- All failed login attempts
CorrelationId IS NOT NULL AND @Message LIKE '%Login%' AND @Level = 'Error'
```

## Testing

Unit test example:

```csharp
[Fact]
public async Task TryHandleAsync_ShouldIncludeCorrelationId()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.Response.Body = new MemoryStream();
    httpContext.Items["CorrelationId"] = "test-correlation-id";

    var exception = new Exception("Test exception");

    // Act
    await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

    // Assert
    httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
    var reader = new StreamReader(httpContext.Response.Body);
    var responseBody = await reader.ReadToEndAsync();

    var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
    response.GetProperty("correlationId").GetString().Should().Be("test-correlation-id");
}
```

## Benefits

| Feature | Benefit |
|---------|---------|
| **Request Tracing** | Track single request across all layers and logs |
| **Debugging** | Filter all logs for specific request in Seq |
| **Client Retry** | Client can retry with same correlation ID for idempotency |
| **Distributed Tracing** | Can be propagated to external services |
| **Clean Architecture** | No HTTP coupling in Application layer |
| **Testability** | Easy to test - just pass string to command |

## Configuration

No additional configuration required. The middleware automatically:
1. Checks `X-Correlation-ID` request header
2. Generates GUID if not present
3. Stores in `HttpContext.Items`
4. Adds to response headers
5. Enriches Serilog context

## Best Practices

1. **Always pass correlation ID to commands** - Even if null, include the parameter
2. **Log at appropriate levels** - Use `LogInformation` for user actions, `LogDebug` for technical details
3. **Include in external API calls** - When calling external services, propagate the correlation ID
4. **Client tracking** - Clients can send correlation ID for retry scenarios

## Future Enhancements

- [ ] Add correlation ID to SignalR messages
- [ ] Propagate to external email service
- [ ] Add correlation ID to background jobs (Hangfire)
- [ ] Implement distributed tracing with OpenTelemetry
