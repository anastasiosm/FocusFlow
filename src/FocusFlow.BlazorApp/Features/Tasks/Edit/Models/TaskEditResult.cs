using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Tasks.Edit.Models;

/// <summary>
/// Result of a task edit operation - used for data transfer
/// </summary>
public record TaskEditResult(
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority
);