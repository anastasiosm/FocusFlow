namespace FocusFlow.BlazorApp.Models.Dtos;

/// <summary>
/// Project response DTO (Blazor version)
/// </summary>
public record ProjectDto(
    Guid Id, 
    string Name, 
    string? Description, 
    string OwnerId, 
    DateTime CreatedAt, 
    DateTime UpdatedAt, 
    int TaskCount);