namespace FocusFlow.BlazorApp.Models.Dtos;

/// <summary>
/// Project statistics DTO (Blazor version)
/// </summary>
public record ProjectStatisticsDto(
    Guid ProjectId,
    string ProjectName,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int TodoTasks,
    int OverdueTasks);