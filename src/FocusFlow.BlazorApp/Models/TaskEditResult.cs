using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

public record TaskEditResult(
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority
);
