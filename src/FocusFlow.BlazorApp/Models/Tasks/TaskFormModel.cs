namespace FocusFlow.BlazorApp.Models.Tasks;

/// <summary>
/// Form model for creating and updating tasks.
/// Used across all task editing scenarios.
/// </summary>
public class TaskFormModel
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTime? DueDate { get; set; }
	public Priority Priority { get; set; } = Priority.Medium;
}
