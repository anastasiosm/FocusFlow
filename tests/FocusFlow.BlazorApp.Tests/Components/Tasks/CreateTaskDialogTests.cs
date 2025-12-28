using Bunit;
using FocusFlow.BlazorApp.Components.Tasks;
using FocusFlow.Domain.Enums;
using FluentAssertions;
using MudBlazor;
using MudBlazor.Services;
using Xunit;
using Microsoft.AspNetCore.Components;
using FocusFlow.BlazorApp.Features.Tasks.Create.Models;

namespace FocusFlow.BlazorApp.Tests.Components.Tasks;

public class CreateTaskDialogTests : TestContext
{
	public CreateTaskDialogTests()
	{
		Services.AddMudServices();

		// Setup all required JSInterop calls for MudBlazor components
		JSInterop.Mode = JSRuntimeMode.Loose; // This allows unhandled calls to pass through
		JSInterop.SetupVoid("mudPopover.initialize", _ => true);
		JSInterop.SetupVoid("mudPopover.connect", _ => true);
		JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
		JSInterop.SetupVoid("mudOverlay.unlockScroll", _ => true);
		JSInterop.SetupVoid("mudOverlay.lockScroll", _ => true);
		JSInterop.SetupVoid("mudScrollManager.unlockScroll", _ => true);
		JSInterop.SetupVoid("mudScrollManager.lockScroll", _ => true);
		JSInterop.SetupVoid("mudElementRef.getBoundingClientRect", _ => true);
		JSInterop.SetupVoid("mudElementRef.saveFocus", _ => true);
		JSInterop.Setup<int>("mudpopoverHelper.countProviders").SetResult(0);
	}

	private (IRenderedComponent<MudDialogProvider> dialogProvider, IRenderedComponent<MudPopoverProvider> popoverProvider) SetupProviders()
	{
		var dialogProvider = RenderComponent<MudDialogProvider>();
		var popoverProvider = RenderComponent<MudPopoverProvider>();
		return (dialogProvider, popoverProvider);
	}

	private DialogService GetDialogService() => Services.GetService<IDialogService>() as DialogService;

	[Fact]
	public void CreateTaskDialog_ShouldRenderCorrectly()
	{
		// Arrange
		var (cut, _) = SetupProviders();
		var dialogService = GetDialogService();

		// Act
		var dialog = dialogService?.Show<CreateTaskDialog>("Create New Task");
		var dialogInstance = cut.FindComponent<CreateTaskDialog>();

		// Assert
		dialog.Should().NotBeNull();
		dialog?.Result.IsCompleted.Should().BeFalse();

		cut.Find(".mud-dialog-title").TextContent.Should().Contain("Create New Task");
		cut.FindAll("label").Should().Contain(x => x.TextContent == "Title");
		cut.FindAll("label").Should().Contain(x => x.TextContent == "Description");
		cut.FindAll("label").Should().Contain(x => x.TextContent == "Due Date");
		cut.FindAll("label").Should().Contain(x => x.TextContent == "Priority");

		cut.Find("[data-testid='cancel-button']").Should().NotBeNull();
		cut.Find("[data-testid='submit-button']").Should().NotBeNull();
	}

	[Fact]
	public async Task CreateTaskDialog_ShouldReturnModelOnValidSubmission()
	{
		// Arrange
		var (cut, _) = SetupProviders();
		var dialogService = GetDialogService();

		var initial = new CreateTaskFormModel
		{
			Title = "Test Task Title",
			Description = "Test Task Description",
			DueDate = DateTime.Today.AddDays(1),
			Priority = Priority.High
		};

		var parameters = new DialogParameters { { "InitialModel", initial } };
		var dialog = dialogService?.Show<CreateTaskDialog>("Create New Task", parameters);

		// Wait for component to render
		cut.WaitForAssertion(() => cut.Find("[data-testid='submit-button']").Should().NotBeNull(), TimeSpan.FromSeconds(2));

		// Find the MudForm component and trigger validation manually
		var dialogComponent = cut.FindComponent<CreateTaskDialog>();
		var formComponent = dialogComponent.FindComponent<MudForm>();

		// Manually validate the form
		await formComponent.InvokeAsync(async () => await formComponent.Instance.Validate());

		// Wait for validation to complete and state to update
		await Task.Delay(300);

		// Act
		var submitButton = cut.Find("[data-testid='submit-button']");
		await submitButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

		// Allow time for the dialog to close
		await Task.Delay(200);

		// Assert
		var resultTask = dialog!.Result;
		var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
		var completedTask = await Task.WhenAny(resultTask, timeoutTask);

		if (completedTask == timeoutTask)
		{
			// Debug info if test fails
			var submitButtonState = cut.Find("[data-testid='submit-button']");
			var isDisabled = submitButtonState.HasAttribute("disabled");
			throw new TimeoutException($"Dialog did not close within 5 seconds. Submit button disabled: {isDisabled}");
		}

		var result = await resultTask;
		result.Canceled.Should().BeFalse();
		result.Data.Should().BeOfType<CreateTaskFormModel>();

		var returnedModel = result.Data as CreateTaskFormModel;
		returnedModel!.Title.Should().Be("Test Task Title");
		returnedModel.Description.Should().Be("Test Task Description");
		returnedModel.DueDate!.Value.Date.Should().Be(DateTime.Today.AddDays(1).Date);
		returnedModel.Priority.Should().Be(Priority.High);
	}

	[Fact]
	public async Task CreateTaskDialog_ShouldNotCloseOnInvalidSubmission()
	{
		// Arrange
		var (cut, _) = SetupProviders();
		var dialogService = GetDialogService();

		var initial = new CreateTaskFormModel
		{
			Title = "", // Invalid - required field
			Description = "Valid Description",
			DueDate = null,
			Priority = Priority.Medium
		};

		var parameters = new DialogParameters { { "InitialModel", initial } };
		var dialog = dialogService?.Show<CreateTaskDialog>("Create New Task", parameters);

		cut.WaitForAssertion(() => cut.Find("[data-testid='submit-button']").Should().NotBeNull());

		// Act
		var submitButton = cut.Find("[data-testid='submit-button']");
		await submitButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

		// Wait to ensure dialog would have closed if validation passed
		await Task.Delay(1000);

		// Assert - Dialog should remain open when validation fails
		dialog!.Result.IsCompleted.Should().BeFalse("Dialog should remain open when validation fails");
		cut.FindAll(".mud-dialog").Should().NotBeEmpty("Dialog should still be rendered");
	}

	[Fact]
	public async Task CreateTaskDialog_ShouldCancelOnCancelButtonClick()
	{
		// Arrange
		var (cut, _) = SetupProviders();
		var dialogService = GetDialogService();

		var initial = new CreateTaskFormModel
		{
			Title = "Any",
			Description = "Any",
			DueDate = null,
			Priority = Priority.Low
		};

		var parameters = new DialogParameters { { "InitialModel", initial } };
		var dialog = dialogService?.Show<CreateTaskDialog>("Create New Task", parameters);

		cut.WaitForAssertion(() => cut.Find("[data-testid='cancel-button']").Should().NotBeNull());

		// Act
		var cancelButton = cut.Find("[data-testid='cancel-button']");
		await cancelButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

		// Assert
		var resultTask = dialog!.Result;
		var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
		var completedTask = await Task.WhenAny(resultTask, timeoutTask);

		if (completedTask == timeoutTask)
		{
			throw new TimeoutException("Dialog did not close within 5 seconds");
		}

		var result = await resultTask;
		result.Canceled.Should().BeTrue();
	}
}