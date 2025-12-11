namespace FocusFlow.Application.Features.Projects.Common;

/// <summary>
/// Project response DTO
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="OwnerId"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
/// <param name="TaskCount"></param>
public record ProjectDto(Guid Id, string Name, string? Description, string OwnerId, DateTime CreatedAt, DateTime UpdatedAt, int TaskCount);
