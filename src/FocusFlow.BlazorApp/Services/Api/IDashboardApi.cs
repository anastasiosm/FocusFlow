using FocusFlow.BlazorApp.Models.Dtos;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface IDashboardApi
{
    [Get("/api/dashboard/statistics")]
    Task<List<ProjectStatisticsDto>> GetDashboardStatisticsAsync();
}