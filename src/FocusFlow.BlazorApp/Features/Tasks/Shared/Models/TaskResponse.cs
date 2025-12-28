using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Tasks.Shared.Models;

/// <summary>
/// Task response DTO (Blazor version)
/// </summary>
public record TaskResponse(
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