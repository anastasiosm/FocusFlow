using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

/// <summary>
/// Request model for updating task status
/// </summary>
public class UpdateTaskStatusRequest
{
    public ProjectTaskStatus Status { get; set; }
}