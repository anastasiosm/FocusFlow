namespace FocusFlow.Application.DTOs;

/// <summary>
/// Create project request DTO
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CreateProjectDto(string Name, string? Description);
