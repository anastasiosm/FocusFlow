using FocusFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FocusFlow.BlazorApp.Models;

public class CreateTaskFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public Priority Priority { get; set; } = Priority.Medium;

    public string? AssignedUserId { get; set; }
}
