using FocusFlow.BlazorApp.Features.Tasks.Edit.Models;
using FocusFlow.BlazorApp.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.Edit.Extensions;

/// <summary>
/// Extension methods for TaskEditResult conversions
/// </summary>
public static class TaskEditResultExtensions
{
    /// <summary>
    /// Converts TaskEditResult to UpdateTaskRequest for API calls.
    /// Centralizes conversion logic in one place.
    /// </summary>
    public static UpdateTaskRequest ToUpdateRequest(this TaskEditResult result)
    {
        return new UpdateTaskRequest
        {
            Title = result.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(result.Description) 
                ? null 
                : result.Description.Trim(),
            DueDate = result.DueDate,
            Priority = result.Priority
        };
    }
}