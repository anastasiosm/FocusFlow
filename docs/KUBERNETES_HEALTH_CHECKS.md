# Kubernetes Health Checks in FocusFlow

## Overview

This document explains the health check implementation in FocusFlow for Kubernetes deployment. Health checks are essential for production deployments as they enable self-healing, high availability, and proper traffic routing.

## What are Health Checks?

Health checks are HTTP endpoints that Kubernetes uses to determine:
- **Is the application alive?** (Liveness)
- **Is the application ready to serve traffic?** (Readiness)
- **What's the overall health status?** (General health)

## FocusFlow Health Check Implementation

### API Health Checks (WebApi)

```csharp
// In StartupExtensions.cs - Service Registration
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"), tags: new[] { "live" });

// Add response caching for health checks
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(5);
    options.Period = TimeSpan.FromSeconds(10);
});

// In Pipeline Configuration
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
```

**What it checks:**
- ✅ **Database Connection**: Can connect to PostgreSQL (via custom DatabaseHealthCheck)
- ✅ **Self Check**: Basic application health
- ✅ **Memory/Threading**: Implicit checks by ASP.NET Core

### Custom Database Health Check

```csharp
// DatabaseHealthCheck.cs - Custom implementation
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FocusFlowDbContext _dbContext;

    public DatabaseHealthCheck(FocusFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a timeout token (5 seconds max)
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts.Token);

            // Executing a simple query is more reliable than CanConnectAsync
            // as it verifies the database can actually process requests.
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", linkedCts.Token);

            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Degraded("Database health check was cancelled");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded("Database health check timed out");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Database connection failed: {ex.Message}", ex);
        }
    }
}
```

**Why custom implementation?**
- More control over database connectivity testing
- Better error messages and logging
- Async operation with cancellation support
- Proper dependency injection integration

### Blazor Health Checks (BlazorApp)

```csharp
// In Program.cs - Service Registration
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Blazor app is running"), tags: new[] { "live" })
    .AddCheck<ApiHealthCheck>("api", tags: new[] { "ready" });

// Add response caching for health checks
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(5);
    options.Period = TimeSpan.FromSeconds(10);
});

// Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Only self-checks for liveness (returns 200 if app is running)
});
```

### Custom API Health Check

```csharp
// ApiHealthCheck.cs - Custom implementation
public class ApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Prevent hanging

            var apiBaseUrl = _configuration.GetValue<string>("ApiBaseUrl") 
                ?? "http://focusflow-api:8080";

            var response = await httpClient.GetAsync(
                $"{apiBaseUrl}/health/ready", // Check ready, not general health
                cancellationToken);
            
            return response.IsSuccessStatusCode 
                ? HealthCheckResult.Healthy($"API is reachable ({apiBaseUrl})")
                : HealthCheckResult.Unhealthy($"API returned {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy($"API unreachable: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("API health check timed out");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"API check failed: {ex.Message}");
        }
    }
}
```

**What it checks:**
- ✅ **Self Check**: Blazor application is running
- ✅ **API Dependency**: Can communicate with the API
- ✅ **SignalR Connection**: Implicit through API check

## Health Check Endpoints

### `/health` - General Health Check

**Purpose**: Overall application health status
**Checks**: All registered health checks
**Usage**: Manual monitoring, dashboards, general health overview

```bash
curl http://localhost:8080/health
```

**Response Examples:**
```json
// Healthy
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456"
}

// Unhealthy
{
  "status": "Unhealthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": {
      "status": "Unhealthy",
      "description": "Database connection failed: ..."
    }
  }
}
```

### `/health/ready` - Readiness Probe

**Purpose**: "Am I ready to serve traffic?"
**Kubernetes Usage**: Readiness probe
**Checks**: All dependencies (database, external APIs)
**Action on Failure**: Pod removed from Service (no traffic routing)

```yaml
# In Kubernetes deployment
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

**When it fails:**
- Database is unreachable
- External API dependencies are down
- Application is starting up
- Configuration issues

### `/health/live` - Liveness Probe

**Purpose**: "Am I still alive?"
**Kubernetes Usage**: Liveness probe
**Checks**: Only basic application health (no external dependencies)
**Action on Failure**: Pod restart

```yaml
# In Kubernetes deployment
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

**When it fails:**
- Application deadlock
- Memory exhaustion
- Thread pool starvation
- Application crash/freeze

## Readiness vs Liveness - Key Differences

| Aspect | Readiness Probe | Liveness Probe |
|--------|----------------|----------------|
| **Purpose** | Ready to serve traffic? | Still alive? |
| **Checks** | Dependencies + Self | Self only |
| **Failure Action** | Remove from Service | Restart Pod |
| **Frequency** | More frequent (5s) | Less frequent (10s) |
| **Initial Delay** | Shorter (10s) | Longer (30s) |
| **Dependencies** | ✅ Database, APIs | ❌ No external deps |

## Practical Scenarios

### Scenario 1: Database Connection Lost

```
┌─────────────────┐    ┌─────────────────┐
│   Liveness      │    │   Readiness     │
│   /health/live  │    │  /health/ready  │
│                 │    │                 │
│   ✅ 200 OK     │    │   ❌ 503 Error  │
│   (App alive)   │    │   (DB down)     │
└─────────────────┘    └─────────────────┘

Kubernetes Actions:
├── Keep pod running (liveness OK)
├── Remove from Service (readiness failed)
└── Result: Pod alive but not serving traffic
```

### Scenario 2: Application Deadlock

```
┌─────────────────┐    ┌─────────────────┐
│   Liveness      │    │   Readiness     │
│   /health/live  │    │  /health/ready  │
│                 │    │                 │
│   ❌ Timeout    │    │   ❌ Timeout    │
│   (App frozen)  │    │   (App frozen)  │
└─────────────────┘    └─────────────────┘

Kubernetes Actions:
├── Restart pod (liveness failed)
├── Remove from Service (readiness failed)
└── Result: Fresh pod restart
```

### Scenario 3: Healthy Application

```
┌─────────────────┐    ┌─────────────────┐
│   Liveness      │    │   Readiness     │
│   /health/live  │    │  /health/ready  │
│                 │    │                 │
│   ✅ 200 OK     │    │   ✅ 200 OK     │
│   (App alive)   │    │   (All deps OK) │
└─────────────────┘    └─────────────────┘

Kubernetes Actions:
├── Keep pod running
├── Route traffic to pod
└── Result: Healthy and serving
```

## Configuration Best Practices

### Timing Configuration

```yaml
# Liveness Probe - Conservative settings
livenessProbe:
  initialDelaySeconds: 30  # Wait for app startup
  periodSeconds: 10        # Check every 10 seconds
  timeoutSeconds: 5        # 5 second timeout
  failureThreshold: 3      # 3 failures = restart
  successThreshold: 1      # 1 success = healthy

# Readiness Probe - More aggressive
readinessProbe:
  initialDelaySeconds: 10  # Start checking sooner
  periodSeconds: 5         # Check every 5 seconds
  timeoutSeconds: 3        # Shorter timeout
  failureThreshold: 3      # 3 failures = remove from service
  successThreshold: 1      # 1 success = ready
```

### Resource Considerations

```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"    # Prevent memory exhaustion
    cpu: "500m"        # Prevent CPU starvation
```

## Monitoring and Debugging

### Check Health Status

```bash
# Direct health check
curl http://localhost:8080/health

# Through Kubernetes port-forward
kubectl port-forward service/focusflow-api-service 8080:8080 -n focusflow
curl http://localhost:8080/health
```

### View Pod Health Events

```bash
# Check pod events
kubectl describe pod <pod-name> -n focusflow

# Check probe failures
kubectl get events --field-selector involvedObject.name=<pod-name> -n focusflow
```

### Common Health Check Failures

```bash
# View pod logs for health check issues
kubectl logs <pod-name> -n focusflow

# Check if probes are configured correctly
kubectl describe deployment focusflow-api -n focusflow
```

## Health Check Flow Diagram

```
┌─────────────────┐
│   Kubernetes    │
│   Scheduler     │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐     ┌─────────────────┐
│  Liveness       │────▶│   Pod Restart   │
│  Probe          │     │   (if failed)   │
│  /health/live   │     └─────────────────┘
└─────────────────┘
          │
          ▼
┌─────────────────┐     ┌─────────────────┐
│  Readiness      │────▶│ Service Traffic │
│  Probe          │     │ Routing Control │
│  /health/ready  │     └─────────────────┘
└─────────────────┘
          │
          ▼
┌─────────────────┐
│   Application   │
│   Endpoints     │
│   /health       │
└─────────────────┘
```

## Benefits of Health Checks

### 1. **High Availability**
- Unhealthy pods automatically removed from traffic
- Only healthy instances serve requests
- Automatic failover to healthy pods

### 2. **Self-Healing**
- Crashed applications restart automatically
- Deadlocked applications get fresh start
- No manual intervention required

### 3. **Zero-Downtime Deployments**
- New pods must pass health checks before receiving traffic
- Old pods continue serving until new ones are ready
- Gradual traffic migration

### 4. **Dependency Management**
- Database issues don't crash the application
- External API failures handled gracefully
- Temporary issues resolved automatically

### 5. **Operational Visibility**
- Clear health status for monitoring
- Detailed failure information
- Integration with monitoring systems

## Troubleshooting Guide

### Health Check Failing

1. **Check application logs**
   ```bash
   kubectl logs deployment/focusflow-api -n focusflow
   ```

2. **Test health endpoint directly**
   ```bash
   kubectl exec -it deployment/focusflow-api -n focusflow -- curl http://localhost:8080/health
   ```

3. **Check database connectivity**
   ```bash
   kubectl exec -it deployment/focusflow-postgres -n focusflow -- pg_isready -U focusflow
   ```

### Pod Restart Loop

1. **Check liveness probe configuration**
   ```bash
   kubectl describe deployment focusflow-api -n focusflow
   ```

2. **Increase initial delay**
   ```yaml
   livenessProbe:
     initialDelaySeconds: 60  # Give more startup time
   ```

3. **Check resource limits**
   ```yaml
   resources:
     limits:
       memory: "1Gi"  # Increase if needed
   ```

### Traffic Not Routing

1. **Check readiness probe**
   ```bash
   kubectl get pods -n focusflow
   # Look for READY column: 0/1 means readiness failed
   ```

2. **Check service endpoints**
   ```bash
   kubectl get endpoints focusflow-api-service -n focusflow
   ```

3. **Test readiness endpoint**
   ```bash
   kubectl exec -it deployment/focusflow-api -n focusflow -- curl http://localhost:8080/health/ready
   ```

## Summary

Health checks are **essential** for production Kubernetes deployments. They provide:

- **Automatic failure detection** and recovery
- **Traffic routing** based on application health
- **Self-healing** capabilities for crashed applications
- **Dependency management** for external services
- **Operational visibility** into application health

The FocusFlow implementation covers all critical aspects:
- Database connectivity (API)
- Inter-service communication (Blazor → API)
- Basic application health (both services)
- Proper separation of liveness vs readiness concerns

This ensures a robust, production-ready deployment that can handle failures gracefully and maintain high availability.