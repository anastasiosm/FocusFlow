using FocusFlow.Domain.Exceptions;

namespace FocusFlow.Domain.Entities;

/// <summary>
/// Represents a project that contains tasks
/// </summary>
public class Project
{
	private readonly List<ProjectTask> _tasks = new();

	public Guid Id { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime UpdatedAt { get; private set; }

	public string Name { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public string OwnerId { get; private set; } = string.Empty;

	// with this approach, the tasks can only be modified through methods on the Project entity.
	public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

	private Project()
	{
		Id = Guid.NewGuid();
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}

	public Project(string name, string? description, string ownerId) : this()
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new FocusFlowValidationException("Project name cannot be empty");
		if (name.Length > 200)
			throw new FocusFlowValidationException("Project name cannot exceed 200 characters");
		if (description?.Length > 2000)
			throw new FocusFlowValidationException("Project description cannot exceed 2000 characters");
		if (string.IsNullOrWhiteSpace(ownerId))
			throw new FocusFlowValidationException("Project must have an owner");

		Name = name.Trim();
		Description = description?.Trim();
		OwnerId = ownerId;
	}

	public void Update(string name, string? description)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new FocusFlowValidationException("Project name cannot be empty");
		if (name.Length > 200)
			throw new FocusFlowValidationException("Project name cannot exceed 200 characters");
		if (description?.Length > 2000)
			throw new FocusFlowValidationException("Project description cannot exceed 2000 characters");

		Name = name.Trim();
		Description = description?.Trim();
		UpdatedAt = DateTime.UtcNow;
	}

	public void AddTask(ProjectTask task)
	{
		if (task.ProjectId != Id)
			throw new FocusFlowBusinessRuleException("Task does not belong to this project");

		_tasks.Add(task);
		UpdatedAt = DateTime.UtcNow;
	}

	public void RemoveTask(ProjectTask task)
	{
		_tasks.Remove(task);
		UpdatedAt = DateTime.UtcNow;
	}
}