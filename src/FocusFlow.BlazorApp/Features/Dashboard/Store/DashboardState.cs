using FocusFlow.BlazorApp.Features.Projects.Shared.Models;
using Fluxor;

namespace FocusFlow.BlazorApp.Features.Dashboard.Store;

/// <summary>
/// State for Dashboard feature
/// </summary>
[FeatureState]
public record DashboardState
{
	public List<ProjectStatisticsDto> Statistics { get; init; }
	public bool IsLoading { get; init; }
	public string? ErrorMessage { get; init; }

	private DashboardState() 
	{
		Statistics = new List<ProjectStatisticsDto>();
	}

	public DashboardState(List<ProjectStatisticsDto> statistics, bool isLoading, string? errorMessage)
	{
		Statistics = statistics;
		IsLoading = isLoading;
		ErrorMessage = errorMessage;
	}
}
