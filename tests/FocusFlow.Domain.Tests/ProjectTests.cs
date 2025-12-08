using FluentAssertions;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;

namespace FocusFlow.Domain.Tests;

public class ProjectTests
{
	#region Constructor Tests

	[Fact]
	public void Constructor_WithValidData_ShouldCreateProject()
	{
		// Arrange
		var name = "Test Project";
		var description = "Test Description";
		var ownerId = "user123";

		// Act
		var project = new Project(name, description, ownerId);

		// Assert
		project.Name.Should().Be(name);
		project.Description.Should().Be(description);
		project.OwnerId.Should().Be(ownerId);
		project.Id.Should().NotBeEmpty();
		project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		project.Tasks.Should().BeEmpty();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Constructor_WithEmptyOrNullName_ShouldThrowException(string invalidName)
	{
		// Act
		Action act = () => new Project(invalidName, "Description", "user123");

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*name cannot be empty*");
	}

	[Fact]
	public void Constructor_WithNameTooLong_ShouldThrowException()
	{
		// Arrange
		var longName = new string('a', 201);

		// Act
		Action act = () => new Project(longName, "Description", "user123");

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
		Action act = () => new Project("Valid Name", longDescription, "user123");

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 2000 characters*");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Constructor_WithEmptyOrNullOwnerId_ShouldThrowException(string invalidOwnerId)
	{
		// Act
		Action act = () => new Project("Valid Name", "Description", invalidOwnerId);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*must have an owner*");
	}

	[Fact]
	public void Constructor_WithWhitespaceInNameAndDescription_ShouldTrim()
	{
		// Arrange
		var nameWithSpaces = "  Test Project  ";
		var descriptionWithSpaces = "  Test Description  ";

		// Act
		var project = new Project(nameWithSpaces, descriptionWithSpaces, "user123");

		// Assert
		project.Name.Should().Be("Test Project");
		project.Description.Should().Be("Test Description");
	}

	#endregion

	#region Update Tests

	[Fact]
	public void Update_WithValidData_ShouldUpdatePropertiesAndTimestamp()
	{
		// Arrange
		var project = new Project("Original Name", "Original Description", "user123");
		var originalUpdatedAt = project.UpdatedAt;
		Thread.Sleep(10);

		// Act
		project.Update("New Name", "New Description");

		// Assert
		project.Name.Should().Be("New Name");
		project.Description.Should().Be("New Description");
		project.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Update_WithEmptyOrNullName_ShouldThrowException(string invalidName)
	{
		// Arrange
		var project = new Project("Original Name", "Description", "user123");

		// Act
		Action act = () => project.Update(invalidName, "New Description");

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*name cannot be empty*");
	}

	[Fact]
	public void Update_WithNameTooLong_ShouldThrowException()
	{
		// Arrange
		var project = new Project("Original Name", "Description", "user123");
		var longName = new string('a', 201);

		// Act
		Action act = () => project.Update(longName, "Description");

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 200 characters*");
	}

	[Fact]
	public void Update_WithDescriptionTooLong_ShouldThrowException()
	{
		// Arrange
		var project = new Project("Original Name", "Description", "user123");
		var longDescription = new string('a', 2001);

		// Act
		Action act = () => project.Update("Valid Name", longDescription);

		// Assert
		act.Should().Throw<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 2000 characters*");
	}

	#endregion

	#region AddTask Tests

	[Fact]
	public void AddTask_WithValidTask_ShouldAddToCollection()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task = new ProjectTask("Task 1", "Description", project.Id);

		// Act
		project.AddTask(task);

		// Assert
		project.Tasks.Should().HaveCount(1);
		project.Tasks.Should().Contain(task);
	}

	[Fact]
	public void AddTask_WithTaskFromDifferentProject_ShouldThrowException()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var differentProjectId = Guid.NewGuid();
		var task = new ProjectTask("Task 1", "Description", differentProjectId);

		// Act
		Action act = () => project.AddTask(task);

		// Assert
		act.Should().Throw<FocusFlowBusinessRuleException>()
			.WithMessage("*does not belong to this project*");
	}

	[Fact]
	public void AddTask_ShouldUpdateTimestamp()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var originalUpdatedAt = project.UpdatedAt;
		Thread.Sleep(10);
		var task = new ProjectTask("Task 1", "Description", project.Id);

		// Act
		project.AddTask(task);

		// Assert
		project.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	#endregion

	#region RemoveTask Tests

	[Fact]
	public void RemoveTask_WithExistingTask_ShouldRemoveFromCollection()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task = new ProjectTask("Task 1", "Description", project.Id);
		project.AddTask(task);

		// Act
		project.RemoveTask(task);

		// Assert
		project.Tasks.Should().BeEmpty();
	}

	[Fact]
	public void RemoveTask_WithNonExistingTask_ShouldNotThrow()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task = new ProjectTask("Task 1", "Description", project.Id);

		// Act
		Action act = () => project.RemoveTask(task);

		// Assert
		act.Should().NotThrow();
	}

	[Fact]
	public void RemoveTask_ShouldUpdateTimestamp()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task = new ProjectTask("Task 1", "Description", project.Id);
		project.AddTask(task);
		var originalUpdatedAt = project.UpdatedAt;
		Thread.Sleep(10);

		// Act
		project.RemoveTask(task);

		// Assert
		project.UpdatedAt.Should().BeAfter(originalUpdatedAt);
	}

	#endregion

	#region Task Collection Tests

	[Fact]
	public void Tasks_ShouldBeReadOnlyCollection()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");

		// Assert
		project.Tasks.Should().BeAssignableTo<IReadOnlyCollection<ProjectTask>>();
	}

	[Fact]
	public void AddTask_MultipleTasks_ShouldMaintainOrder()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task1 = new ProjectTask("Task 1", "Desc 1", project.Id);
		var task2 = new ProjectTask("Task 2", "Desc 2", project.Id);
		var task3 = new ProjectTask("Task 3", "Desc 3", project.Id);

		// Act
		project.AddTask(task1);
		project.AddTask(task2);
		project.AddTask(task3);

		// Assert
		project.Tasks.Should().HaveCount(3);
		project.Tasks.Should().ContainInOrder(task1, task2, task3);
	}

	[Fact]
	public void AddTask_DuplicateTask_ShouldHandleAppropriately()
	{
		// Arrange
		var project = new Project("Project", "Description", "user123");
		var task = new ProjectTask("Task 1", "Description", project.Id);
		project.AddTask(task);

		// Act
		Action act = () => project.AddTask(task);

		// Assert - Decide behavior: throw or allow duplicates
		act.Should().NotThrow(); // Or .Throw<FocusFlowBusinessRuleException>()
	}

	#endregion

	#region Edge Cases

	[Fact]
	public void Update_WithNullDescription_ShouldAllowNull()
	{
		// Arrange
		var project = new Project("Project", "Original", "user123");

		// Act
		project.Update("Updated", null);

		// Assert
		project.Description.Should().BeNull();
	}

	[Fact]
	public void Constructor_WithNullDescription_ShouldCreateProject()
	{
		// Arrange & Act
		var project = new Project("Project", null, "user123");

		// Assert
		project.Description.Should().BeNull();
	}

	#endregion
}