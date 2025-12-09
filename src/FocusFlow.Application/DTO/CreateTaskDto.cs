using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.DTO;

public record CreateTaskDto(
    Guid ProjectId,
    string Title,
    string? Description,
    DateTime DueDate,
    Priority Priority,
    string? AssignedUserId);
