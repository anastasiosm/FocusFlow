namespace FocusFlow.BlazorApp.Features.Projects.List.Models;

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