using FocusFlow.BlazorApp.Features.Tasks.Create.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.Create.Extensions;

/// <summary>
/// Extension methods for CreateTaskFormModel conversions
/// </summary>
public static class CreateTaskFormModelExtensions
{
    /// <summary>
    /// Converts CreateTaskFormModel to CreateTaskRequest for API calls.
    /// Centralizes conversion logic in one place.
    /// </summary>
    public static CreateTaskRequest ToCreateRequest(this CreateTaskFormModel formModel, Guid projectId)
    {
        return new CreateTaskRequest
        {
            ProjectId = projectId,
            Title = formModel.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(formModel.Description) 
                ? null 
                : formModel.Description.Trim(),
            DueDate = formModel.DueDate,
            Priority = formModel.Priority,
            AssignedUserId = formModel.AssignedUserId
        };
    }
}