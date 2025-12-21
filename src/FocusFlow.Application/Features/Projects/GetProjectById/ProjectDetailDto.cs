using FocusFlow.Application.Features.Tasks.Common;

namespace FocusFlow.Application.Features.Projects.GetProjectById;

/// <summary>
/// Project with tasks response DTO
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="OwnerId"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
/// <param name="Tasks"></param>
public record ProjectDetailDto(Guid Id, string Name, string? Description, string OwnerId, DateTime CreatedAt, DateTime UpdatedAt, List<TaskDto> Tasks);
