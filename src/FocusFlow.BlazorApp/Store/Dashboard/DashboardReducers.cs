using Fluxor;

namespace FocusFlow.BlazorApp.Store.Dashboard;

/// <summary>
/// Reducers for Dashboard state
/// </summary>
public static class DashboardReducers
{
	[ReducerMethod]
	public static DashboardState OnLoadDashboardStatistics(DashboardState state, DashboardActions.LoadDashboardStatistics action)
	{
		return state with
		{
			IsLoading = true,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static DashboardState OnLoadDashboardStatisticsSuccess(DashboardState state, DashboardActions.LoadDashboardStatisticsSuccess action)
	{
		return state with
		{
			Statistics = action.Statistics,
			IsLoading = false,
			ErrorMessage = null
		};
	}

	[ReducerMethod]
	public static DashboardState OnLoadDashboardStatisticsFailure(DashboardState state, DashboardActions.LoadDashboardStatisticsFailure action)
	{
		return state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage
		};
	}
}
