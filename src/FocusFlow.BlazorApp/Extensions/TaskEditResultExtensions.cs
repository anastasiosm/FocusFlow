using FocusFlow.BlazorApp.Models;
using FocusFlow.BlazorApp.Models.Tasks;

namespace FocusFlow.BlazorApp.Extensions;

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

    /// <summary>
    /// Converts TaskEditResult to UpdateTaskDto for IApiService.
    /// Used by Fluxor effects and other internal services.
    /// </summary>
    public static UpdateTaskDto ToUpdateDto(this TaskEditResult result)
    {
        return new UpdateTaskDto(
            result.Title.Trim(),
            string.IsNullOrWhiteSpace(result.Description) ? null : result.Description.Trim(),
            result.DueDate,
            result.Priority
        );
    }
}