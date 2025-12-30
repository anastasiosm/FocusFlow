using Microsoft.Extensions.Diagnostics.HealthChecks;
using FocusFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.WebApi.HealthChecks;

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