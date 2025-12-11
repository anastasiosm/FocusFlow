using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Components.Tasks;

public class TaskFormModel
{
    public Guid ProjectId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string? AssignedUserId { get; set; }

    public CreateTaskCommand ToCommand()
        => new(ProjectId, Title, Description, DueDate, Priority, AssignedUserId);
}