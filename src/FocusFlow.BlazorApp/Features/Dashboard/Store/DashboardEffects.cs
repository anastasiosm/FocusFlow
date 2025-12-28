using Fluxor;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Features.Dashboard.Store;

/// <summary>
/// Effects for Dashboard feature
/// </summary>
public class DashboardEffects
{
	private readonly IApiService _apiService;
	private readonly ILogger<DashboardEffects> _logger;

	public DashboardEffects(IApiService apiService, ILogger<DashboardEffects> logger)
	{
		_apiService = apiService;
		_logger = logger;
	}

	[EffectMethod]
	public async Task HandleLoadDashboardStatistics(DashboardActions.LoadDashboardStatistics action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Loading dashboard statistics...");

		var result = await _apiService.GetDashboardStatisticsAsync();

		if (result.Succeeded)
		{
			_logger.LogInformation("Successfully loaded {Count} project statistics", result.Data?.Count ?? 0);
			dispatcher.Dispatch(new DashboardActions.LoadDashboardStatisticsSuccess(result.Data ?? new()));
		}
		else
		{
			_logger.LogError("Failed to load dashboard statistics: {Error}", result.Error);
			dispatcher.Dispatch(new DashboardActions.LoadDashboardStatisticsFailure(result.Error ?? "Unknown error"));
		}
	}
}
