using FluentAssertions;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;

namespace FocusFlow.Domain.Tests;

public class ProjectTaskTests
{
	#region Constructor Tests

	[Fact]
	public void Constructor_WithValidData_ShouldCreateTask()
	{
		// Arrange
		var title = "Test Task";
		var description = "Test Description";
		var projectId = Guid.NewGuid();
		var dueDate = DateTime.UtcNow.AddDays(7);
		var priority = Priority.High;
		var assignedUserId = "user123";

		// Act
		var task = new ProjectTask(title, description, projectId, dueDate, priority, assignedUserId);

		// Assert
		task.Title.Should().Be(title);
		task.Description.Should().Be(description);
		task.ProjectId.Should().Be(projectId);
		task.DueDate.Should().BeCloseTo(dueDate, TimeSpan.FromSeconds(1));
		task.Priority.Should().Be(priority);
		task.AssignedUserId.Should().Be(assignedUserId);
		task.Id.Should().NotBeEmpty();
		task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		task.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Constructor_WithEmptyOrNullTitle_ShouldThrowException(string invalidTitle)
	{
		// Act
		Action act = () => new ProjectTask(invalidTitle, "Description", Guid.NewGuid());

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*title cannot be empty*");
	}

	[Fact]
	public void Constructor_WithTitleTooLong_ShouldThrowException()
	{
		// Arrange
		var longTitle = new string('a', 201);

		// Act
		Action act = () => new ProjectTask(longTitle, "Description", Guid.NewGuid());

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 200 characters*");
	}

	[Fact]
	public void Constructor_WithDescriptionTooLong_ShouldThrowException()
	{
		// Arrange
		var longDescription = new string('a', 2001);

		// Act
		Action act = () => new ProjectTask("Valid Title", longDescription, Guid.NewGuid());

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 2000 characters*");
	}

	[Fact]
	public void Constructor_WithDefaultValues_ShouldSetCorrectDefaults()
	{
		// Act
		var task = new ProjectTask("Title", null, Guid.NewGuid());

		// Assert
		task.Status.Should().Be(ProjectTaskStatus.Todo);
		task.Priority.Should().Be(Priority.Medium);
		task.DueDate.Should().BeNull();
		task.AssignedUserId.Should().BeNull();
		task.CompletedAt.Should().BeNull();
	}

	[Fact]
	public void Constructor_WithDueDate_ShouldConvertToUtc()
	{
		// Arrange
		var localDate = new DateTime(2024, 12, 31, 10, 0, 0, DateTimeKind.Local);

		// Act
		var task = new ProjectTask("Title", null, Guid.NewGuid(), localDate);

		// Assert
		task.DueDate.Should().NotBeNull();
		task.DueDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
	}

	[Fact]
	public void Constructor_WithWhitespaceInTitleAndDescription_ShouldTrim()
	{
		// Arrange
		var titleWithSpaces = "  Test Task  ";
		var descriptionWithSpaces = "  Test Description  ";

		// Act
		var task = new ProjectTask(titleWithSpaces, descriptionWithSpaces, Guid.NewGuid());

		// Assert
		task.Title.Should().Be("Test Task");
		task.Description.Should().Be("Test Description");
	}

	[Fact]
	public void Constructor_WithAssignedUserId_ShouldSetAssignedUserId()
	{
		// Act
		var task = new ProjectTask("Title", null, Guid.NewGuid(), null, Priority.Medium, "user456");

		// Assert
		task.AssignedUserId.Should().Be("user456");
	}

	[Fact]
	public void Constructor_WithEmptyProjectId_ShouldThrowException()
	{
		// Act
		Action act = () => new ProjectTask("Valid Title", "Description", Guid.Empty);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*Project ID cannot be empty*");
	}

	#endregion

	#region Update Tests

	[Fact]
	public void Update_WithValidData_ShouldUpdatePropertiesAndTimestamp()
	{
		// Arrange
		var task = new ProjectTask("Original Title", "Original Description", Guid.NewGuid());
		var originalUpdatedAt = task.UpdatedAt;
		Thread.Sleep(10);
		var newDueDate = DateTime.UtcNow.AddDays(5);

		// Act
		task.Update("New Title", "New Description", newDueDate, Priority.Critical);

		// Assert
		task.Title.Should().Be("New Title");
		task.Description.Should().Be("New Description");
		task.DueDate.Should().BeCloseTo(newDueDate, TimeSpan.FromSeconds(1));
		task.Priority.Should().Be(Priority.Critical);
		task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Update_WithEmptyOrNullTitle_ShouldThrowException(string invalidTitle)
	{
		// Arrange
		var task = new ProjectTask("Original Title", "Description", Guid.NewGuid());

		// Act
		Action act = () => task.Update(invalidTitle, "New Description", null, Priority.Medium);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*title cannot be empty*");
	}

	[Fact]
	public void Update_WithTitleTooLong_ShouldThrowException()
	{
		// Arrange
		var task = new ProjectTask("Original Title", "Description", Guid.NewGuid());
		var longTitle = new string('a', 201);

		// Act
		Action act = () => task.Update(longTitle, "Description", null, Priority.Medium);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 200 characters*");
	}

	[Fact]
	public void Update_WithDescriptionTooLong_ShouldThrowException()
	{
		// Arrange
		var task = new ProjectTask("Original Title", "Description", Guid.NewGuid());
		var longDescription = new string('a', 2001);

		// Act
		Action act = () => task.Update("Valid Title", longDescription, null, Priority.Medium);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 2000 characters*");
	}

	[Fact]
	public void Update_WithNullDueDate_ShouldClearDueDate()
	{
		// Arrange
		var originalDueDate = DateTime.UtcNow.AddDays(7);
		var task = new ProjectTask("Task", null, Guid.NewGuid(), originalDueDate);

		// Act
		task.Update("Updated Task", "Updated Description", null, Priority.Low);

		// Assert
		task.DueDate.Should().BeNull();
	}

	[Fact]
	public void Update_WithNullDescription_ShouldAllowNull()
	{
		// Arrange
		var task = new ProjectTask("Task", "Original Description", Guid.NewGuid());

		// Act
		task.Update("Updated Task", null, null, Priority.Medium);

		// Assert
		task.Description.Should().BeNull();
	}

	#endregion

	#region SetStatus Tests

	[Fact]
	public void SetStatus_FromTodoToInProgress_ShouldChangeStatus()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		// Act
		task.SetStatus(ProjectTaskStatus.InProgress);

		// Assert
		task.Status.Should().Be(ProjectTaskStatus.InProgress);
		task.CompletedAt.Should().BeNull();
	}

	[Fact]
	public void SetStatus_FromInProgressToDone_ShouldSetCompletedAt()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		task.SetStatus(ProjectTaskStatus.InProgress);

		// Act
		task.SetStatus(ProjectTaskStatus.Done);

		// Assert
		task.Status.Should().Be(ProjectTaskStatus.Done);
		task.CompletedAt.Should().NotBeNull();
		task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void SetStatus_FromDoneToTodo_ShouldThrowException()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		task.SetStatus(ProjectTaskStatus.Done);

		// Act
		Action act = () => task.SetStatus(ProjectTaskStatus.Todo);

		// Assert
		act.Should().Throw<FocusFlowBusinessRuleException>()
			.WithMessage("*Cannot reopen a completed task*");
	}

	[Fact]
	public void SetStatus_FromDoneToInProgress_ShouldThrowException()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		task.SetStatus(ProjectTaskStatus.Done);

		// Act
		Action act = () => task.SetStatus(ProjectTaskStatus.InProgress);

		// Assert
		act.Should().Throw<FocusFlowBusinessRuleException>()
			.WithMessage("*Cannot reopen a completed task*");
	}

	[Fact]
	public void SetStatus_ToNonDoneStatus_ShouldClearCompletedAt()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		// Act
		task.SetStatus(ProjectTaskStatus.InProgress);

		// Assert
		task.CompletedAt.Should().BeNull();
	}

	[Fact]
	public void SetStatus_ShouldUpdateTimestamp()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		var originalUpdatedAt = task.UpdatedAt;
		Thread.Sleep(10);

		// Act
		task.SetStatus(ProjectTaskStatus.InProgress);

		// Assert
		task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	#endregion

	#region Assign/Unassign Tests

	[Fact]
	public void Assign_WithValidUserId_ShouldSetAssignedUserId()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		var userId = "user123";

		// Act
		task.Assign(userId);

		// Assert
		task.AssignedUserId.Should().Be(userId);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Assign_WithEmptyOrNullUserId_ShouldThrowException(string invalidUserId)
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		// Act
		Action act = () => task.Assign(invalidUserId);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*User ID cannot be empty*");
	}

	[Fact]
	public void Assign_ShouldUpdateTimestamp()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		var originalUpdatedAt = task.UpdatedAt;
		Thread.Sleep(10);

		// Act
		task.Assign("user123");

		// Assert
		task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	[Fact]
	public void Unassign_ShouldClearAssignedUserId()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		task.Assign("user123");

		// Act
		task.Unassign();

		// Assert
		task.AssignedUserId.Should().BeNull();
	}

	[Fact]
	public void Unassign_ShouldUpdateTimestamp()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());
		task.Assign("user123");
		var originalUpdatedAt = task.UpdatedAt;
		Thread.Sleep(10);

		// Act
		task.Unassign();

		// Assert
		task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	#endregion

	#region IsOverdue Tests

	[Fact]
	public void IsOverdue_WithPastDueDateAndNotDone_ShouldReturnTrue()
	{
		// Arrange
		var pastDueDate = DateTime.UtcNow.AddDays(-1);
		var task = new ProjectTask("Task", null, Guid.NewGuid(), pastDueDate);

		// Act
		var isOverdue = task.IsOverdue();

		// Assert
		isOverdue.Should().BeTrue();
	}

	[Fact]
	public void IsOverdue_WithFutureDueDate_ShouldReturnFalse()
	{
		// Arrange
		var futureDueDate = DateTime.UtcNow.AddDays(7);
		var task = new ProjectTask("Task", null, Guid.NewGuid(), futureDueDate);

		// Act
		var isOverdue = task.IsOverdue();

		// Assert
		isOverdue.Should().BeFalse();
	}

	[Fact]
	public void IsOverdue_WithNoDueDate_ShouldReturnFalse()
	{
		// Arrange
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		// Act
		var isOverdue = task.IsOverdue();

		// Assert
		isOverdue.Should().BeFalse();
	}

	[Fact]
	public void IsOverdue_WithPastDueDateButCompleted_ShouldReturnFalse()
	{
		// Arrange
	var pastDueDate = DateTime.UtcNow.AddDays(-1);
	var task = new ProjectTask("Task", null, Guid.NewGuid(), pastDueDate);
	task.SetStatus(ProjectTaskStatus.Done);

		// Act
		var isOverdue = task.IsOverdue();

		// Assert
		isOverdue.Should().BeFalse();
	}

	#endregion
}