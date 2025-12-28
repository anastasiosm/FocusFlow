using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Tasks.UpdateStatus.Models;

/// <summary>
/// Request model for updating task status
/// </summary>
public class UpdateTaskStatusRequest
{
    public ProjectTaskStatus Status { get; set; }
}