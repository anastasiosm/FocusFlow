using FocusFlow.Application.Features.Dashboard.Common;
using MediatR;

namespace FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;

/// <summary>
/// Query to get dashboard statistics for user's projects
/// </summary>
/// <param name="UserId">User ID to get statistics for</param>
public record GetDashboardStatisticsQuery(string UserId) : IRequest<List<ProjectStatisticsDto>>;
