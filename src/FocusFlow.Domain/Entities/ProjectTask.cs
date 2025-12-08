using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;

namespace FocusFlow.Domain.Entities;

/// <summary>
/// Represents a task within a project
/// </summary>
public class ProjectTask
{
	public Guid Id { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime UpdatedAt { get; private set; }

	public string Title { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public DateTime? DueDate { get; private set; }
	public ProjectTaskStatus Status { get; private set; }
	public Enums.Priority Priority { get; private set; }
	public DateTime? CompletedAt { get; private set; }

	public Guid ProjectId { get; private set; }
	public Project? Project { get; private set; }

	public string? AssignedUserId { get; private set; }

	private ProjectTask()
	{
		Id = Guid.NewGuid();
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}

	public ProjectTask(string title, string? description, Guid projectId, DateTime? dueDate = null, Enums.Priority priority = Enums.Priority.Medium, string? assignedUserId = null) : this()
	{
		if (projectId == Guid.Empty)
			throw new FocusFlowValidationException("Project ID cannot be empty");
		if (string.IsNullOrWhiteSpace(title))
			throw new FocusFlowValidationException("Task title cannot be empty");
		if (title.Length > 200)
			throw new FocusFlowValidationException("Task title cannot exceed 200 characters");
		if (description?.Length > 2000)
			throw new FocusFlowValidationException("Task description cannot exceed 2000 characters");

		Title = title.Trim();
		Description = description?.Trim();
		ProjectId = projectId;
		DueDate = dueDate?.ToUniversalTime();
		Priority = priority;
		Status = ProjectTaskStatus.Todo;
		AssignedUserId = assignedUserId;
	}

	public void Update(string title, string? description, DateTime? dueDate, Enums.Priority priority)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new FocusFlowValidationException("Task title cannot be empty");
		if (title.Length > 200)
			throw new FocusFlowValidationException("Task title cannot exceed 200 characters");
		if (description?.Length > 2000)
			throw new FocusFlowValidationException("Task description cannot exceed 2000 characters");

		Title = title.Trim();
		Description = description?.Trim();
		DueDate = dueDate?.ToUniversalTime();
		Priority = priority;
		UpdatedAt = DateTime.UtcNow;
	}

	public void SetStatus(ProjectTaskStatus status)
	{
		if (Status == ProjectTaskStatus.Done && status != ProjectTaskStatus.Done)
			throw new FocusFlowBusinessRuleException("Cannot reopen a completed task");

		Status = status;
		CompletedAt = status == ProjectTaskStatus.Done ? DateTime.UtcNow : null;
		UpdatedAt = DateTime.UtcNow;
	}

	public void Assign(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
			throw new FocusFlowValidationException("User ID cannot be empty");

		AssignedUserId = userId;
		UpdatedAt = DateTime.UtcNow;
	}

	public void Unassign()
	{
		AssignedUserId = null;
		UpdatedAt = DateTime.UtcNow;
	}

	public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != ProjectTaskStatus.Done;
}