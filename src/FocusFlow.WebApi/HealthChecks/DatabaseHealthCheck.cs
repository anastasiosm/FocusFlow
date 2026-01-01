using FocusFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace FocusFlow.WebApi.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
	private readonly FocusFlowDbContext _dbContext;
	private readonly ILogger<DatabaseHealthCheck> _logger;

	public DatabaseHealthCheck(
		FocusFlowDbContext dbContext,
		ILogger<DatabaseHealthCheck> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		var stopwatch = Stopwatch.StartNew();

		try
		{
			using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken,
				timeoutCts.Token);

			// Executing a simple query is more reliable than CanConnectAsync
			// as it verifies the database can actually process requests.
			await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", linkedCts.Token);

			stopwatch.Stop();

			var data = new Dictionary<string, object>
			{
				{ "response_time_ms", stopwatch.ElapsedMilliseconds },
				{ "database_provider", _dbContext.Database.ProviderName ?? "Unknown" }
			};

			_logger.LogDebug(
				"[OK] Database health check passed. ResponseTime: {ResponseTime}ms",
				stopwatch.ElapsedMilliseconds);

			return HealthCheckResult.Healthy(
				$"Database connection is healthy (response time: {stopwatch.ElapsedMilliseconds}ms)",
				data);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			stopwatch.Stop();
			_logger.LogWarning("[PAUSE] Database health check was cancelled");

			return HealthCheckResult.Degraded(
				"Database health check was cancelled",
				data: new Dictionary<string, object>
				{
					{ "response_time_ms", stopwatch.ElapsedMilliseconds }
				});
		}
		catch (OperationCanceledException)
		{
			stopwatch.Stop();
			_logger.LogError("[TIMEOUT] Database health check timed out after {Timeout}ms", stopwatch.ElapsedMilliseconds);

			return HealthCheckResult.Degraded(
				$"Database health check timed out after {stopwatch.ElapsedMilliseconds}ms",
				data: new Dictionary<string, object>
				{
					{ "response_time_ms", stopwatch.ElapsedMilliseconds },
					{ "timeout", true }
				});
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "[ERROR] Database connection failed. Error: {Error}", ex.Message);

			return HealthCheckResult.Unhealthy(
				$"Database connection failed: {ex.Message}",
				ex,
				new Dictionary<string, object>
				{
					{ "response_time_ms", stopwatch.ElapsedMilliseconds },
					{ "error", ex.Message }
				});
		}
	}
}