using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

/// <summary>
/// DTO for updating a task
/// </summary>
public record UpdateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    Priority Priority);