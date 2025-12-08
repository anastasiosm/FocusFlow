using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.DTO;

/// <summary>
/// Task response DTO
/// </summary>
/// <param name="Id"></param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="DueDate"></param>
/// <param name="Status"></param>
/// <param name="Priority"></param>
/// <param name="CompletedAt"></param>
/// <param name="ProjectId"></param>
/// <param name="AssignedUserId"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public record TaskDto(Guid Id, string Title, string? Description, DateTime? DueDate, Domain.Enums.ProjectTaskStatus Status, Priority Priority, DateTime? CompletedAt, Guid ProjectId, string? AssignedUserId, DateTime CreatedAt, DateTime UpdatedAt);
