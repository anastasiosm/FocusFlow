namespace FocusFlow.BlazorApp.Models.Tasks;

/// <summary>
/// Result of a task edit operation - used for data transfer
/// </summary>
public record TaskEditResult(
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority
);