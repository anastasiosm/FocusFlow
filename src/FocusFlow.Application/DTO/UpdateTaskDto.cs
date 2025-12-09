using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.DTO;

public record UpdateTaskDto(
    string Title,
    string? Description,
    DateTime DueDate,
    Priority Priority);
