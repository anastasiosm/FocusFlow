namespace FocusFlow.Application.Features.Projects.UpdateProject;

/// <summary>
/// Update project request DTO
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record UpdateProjectDto(string Name, string? Description);
