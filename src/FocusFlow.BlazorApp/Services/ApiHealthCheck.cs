using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ApiHealthCheck : IHealthCheck
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;

	public ApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
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