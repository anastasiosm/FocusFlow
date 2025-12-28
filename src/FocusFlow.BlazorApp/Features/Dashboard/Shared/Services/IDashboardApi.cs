using FocusFlow.BlazorApp.Features.Projects.Shared.Models;
using Refit;

namespace FocusFlow.BlazorApp.Features.Dashboard.Shared.Services;

public interface IDashboardApi
{
    [Get("/api/dashboard/statistics")]
    Task<List<ProjectStatisticsDto>> GetDashboardStatisticsAsync();
}