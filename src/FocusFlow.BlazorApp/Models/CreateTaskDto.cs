using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

/// <summary>
/// DTO for creating a task (Blazor internal)
/// </summary>
public record CreateTaskDto(
    Guid ProjectId,
    string Title,
    string? Description,
    DateTime? DueDate,
    Priority Priority,
    string? AssignedUserId);