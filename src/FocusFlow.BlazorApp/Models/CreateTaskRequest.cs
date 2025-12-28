using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

/// <summary>
/// Request model for creating a task (API contract)
/// </summary>
public class CreateTaskRequest
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public string? AssignedUserId { get; set; }
}