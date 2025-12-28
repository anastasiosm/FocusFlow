namespace FocusFlow.BlazorApp.Models.Dtos;

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
    List<TaskDto> Tasks);