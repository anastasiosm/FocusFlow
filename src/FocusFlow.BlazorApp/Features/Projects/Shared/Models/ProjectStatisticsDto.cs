namespace FocusFlow.BlazorApp.Features.Projects.Shared.Models;

/// <summary>
/// Project statistics for dashboard (Blazor version)
/// </summary>
/// <param name="ProjectId">Project identifier</param>
/// <param name="ProjectName">Project name</param>
/// <param name="TotalTasks">Total number of tasks</param>
/// <param name="CompletedTasks">Number of completed tasks</param>
/// <param name="OverdueTasks">Number of overdue tasks</param>
public record ProjectStatisticsDto(
	Guid ProjectId,
	string ProjectName,
	int TotalTasks,
	int CompletedTasks,
	int OverdueTasks);