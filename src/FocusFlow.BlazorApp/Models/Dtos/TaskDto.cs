using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models.Dtos;

/// <summary>
/// Task response DTO (Blazor version)
/// </summary>
public record TaskDto(
    Guid Id, 
    string Title, 
    string? Description, 
    DateTime? DueDate, 
    ProjectTaskStatus Status, 
    Priority Priority, 
    DateTime? CompletedAt, 
    Guid ProjectId, 
    string? AssignedUserId, 
    DateTime CreatedAt, 
    DateTime UpdatedAt);