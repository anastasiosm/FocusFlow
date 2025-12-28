using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Models;

/// <summary>
/// Project detail response DTO (Blazor version)
/// </summary>
public record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TaskResponse> Tasks);