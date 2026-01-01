using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace FocusFlow.BlazorApp.Services;

public class ApiHealthCheck : IHealthCheck
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly ILogger<ApiHealthCheck> _logger;

	public ApiHealthCheck(
		IHttpClientFactory httpClientFactory,
		IConfiguration configuration,
		ILogger<ApiHealthCheck> logger)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		var apiBaseUrl = _configuration.GetValue<string>("ApiBaseUrl") ?? "http://focusflow-api:8080";
		var stopwatch = Stopwatch.StartNew();

		try
		{
			var httpClient = _httpClientFactory.CreateClient();
			httpClient.Timeout = TimeSpan.FromSeconds(5);  // Prevent hanging

			using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken,
				timeoutCts.Token);

			var response = await httpClient.GetAsync(
				$"{apiBaseUrl}/health/ready",
				linkedCts.Token);

			stopwatch.Stop();

			var data = new Dictionary<string, object>
			{
				{ "api_url", apiBaseUrl },
				{ "response_time_ms", stopwatch.ElapsedMilliseconds },
				{ "status_code", (int)response.StatusCode }
			};

			if (response.IsSuccessStatusCode)
			{
				_logger.LogDebug(
					"[OK] API health check passed. URL: {Url}, ResponseTime: {ResponseTime}ms",
					apiBaseUrl,
					stopwatch.ElapsedMilliseconds);

				return HealthCheckResult.Healthy(
					$"API is reachable at {apiBaseUrl} (response time: {stopwatch.ElapsedMilliseconds}ms)",
					data);
			}
			else
			{
				_logger.LogWarning(
					"[WARN] API health check failed. URL: {Url}, StatusCode: {StatusCode}, ResponseTime: {ResponseTime}ms",
					apiBaseUrl,
					response.StatusCode,
					stopwatch.ElapsedMilliseconds);

				return HealthCheckResult.Unhealthy(
					$"API returned {response.StatusCode}",
					data: data);
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			stopwatch.Stop();
			_logger.LogWarning("[PAUSE] API health check was cancelled. ResponseTime: {ResponseTime}ms", stopwatch.ElapsedMilliseconds);

			return HealthCheckResult.Degraded(
				"API health check was cancelled",
				data: new Dictionary<string, object>
				{
					{ "api_url", apiBaseUrl },
					{ "response_time_ms", stopwatch.ElapsedMilliseconds }
				});
		}
		catch (OperationCanceledException)
		{
			stopwatch.Stop();
			_logger.LogError("[TIMEOUT] API health check timed out after {Timeout}ms", stopwatch.ElapsedMilliseconds);

			return HealthCheckResult.Degraded(
				$"API health check timed out after {stopwatch.ElapsedMilliseconds}ms",
				data: new Dictionary<string, object>
				{
					{ "api_url", apiBaseUrl },
					{ "response_time_ms", stopwatch.ElapsedMilliseconds },
					{ "timeout", true }
				});
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "[ERROR] API is unreachable. URL: {Url}, Error: {Error}", apiBaseUrl, ex.Message);

			return HealthCheckResult.Unhealthy(
				$"API is unreachable: {ex.Message}",
				ex,
				new Dictionary<string, object>
				{
					{ "api_url", apiBaseUrl },
					{ "response_time_ms", stopwatch.ElapsedMilliseconds },
					{ "error", ex.Message }
				});
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "[ERROR] API health check failed unexpectedly. URL: {Url}", apiBaseUrl);

			return HealthCheckResult.Unhealthy(
				$"API health check failed: {ex.Message}",
				ex,
				new Dictionary<string, object>
				{
					{ "api_url", apiBaseUrl },
					{ "response_time_ms", stopwatch.ElapsedMilliseconds },
					{ "error", ex.Message }
				});
		}
	}
}