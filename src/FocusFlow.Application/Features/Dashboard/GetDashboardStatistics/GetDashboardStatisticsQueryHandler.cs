using FocusFlow.Application.Features.Dashboard.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;

/// <summary>
/// Handler for GetDashboardStatisticsQuery
/// </summary>
public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, List<ProjectStatisticsDto>>
{
	private readonly IProjectRepository _projectRepository;

	public GetDashboardStatisticsQueryHandler(IProjectRepository projectRepository)
	{
		_projectRepository = projectRepository;
	}

	public async Task<List<ProjectStatisticsDto>> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
	{
		// Get all projects with tasks for the user
		var projects = await _projectRepository.GetByOwnerIdWithTasksAsync(request.UserId, cancellationToken);

		var statistics = projects.Select(project =>
		{
			var tasks = project.Tasks.ToList();
			var totalTasks = tasks.Count;
			var completedTasks = tasks.Count(t => t.Status == ProjectTaskStatus.Done);
			var overdueTasks = tasks.Count(t => t.IsOverdue());

			return new ProjectStatisticsDto(
				project.Id,
				project.Name,
				totalTasks,
				completedTasks,
				overdueTasks);
		}).ToList();

		return statistics;
	}
}
