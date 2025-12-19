using FocusFlow.Application.Features.Dashboard.Common;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface IDashboardApi
{
    [Get("/api/dashboard/statistics")]
    Task<List<ProjectStatisticsDto>> GetDashboardStatisticsAsync();
}