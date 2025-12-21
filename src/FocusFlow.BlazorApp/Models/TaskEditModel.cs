using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Models;

public class TaskEditModel
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTime? DueDate { get; set; }
	public Priority Priority { get; set; } = Priority.Medium;
}
