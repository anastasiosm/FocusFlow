using FocusFlow.BlazorApp.Models.Dtos;

namespace FocusFlow.BlazorApp.Store.Dashboard;

/// <summary>
/// Actions for Dashboard feature
/// </summary>
public static class DashboardActions
{
	public record LoadDashboardStatistics;
	public record LoadDashboardStatisticsSuccess(List<ProjectStatisticsDto> Statistics);
	public record LoadDashboardStatisticsFailure(string ErrorMessage);
}
