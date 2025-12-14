using Bunit;
using FocusFlow.BlazorApp.Components.Pages;
using FocusFlow.BlazorApp.Models;
using FocusFlow.BlazorApp.Models.Validators;
using FocusFlow.BlazorApp.Store.Auth;
using Fluxor;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using FluentAssertions;

namespace FocusFlow.BlazorApp.Tests.Components.Pages;

public class LoginTests : TestContextBase
{
    private readonly IDispatcher _mockDispatcher;
    private readonly IState<AuthState> _mockAuthState;
    private readonly ISnackbar _mockSnackbar;

    public LoginTests()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockAuthState = Substitute.For<IState<AuthState>>();
        _mockSnackbar = Substitute.For<ISnackbar>();
        
        _mockAuthState.Value.Returns(new AuthState(
            isLoading: false,
            isAuthenticated: false,
            username: null,
            token: null,
            error: null
        ));

        Services.AddSingleton(_mockDispatcher);
        Services.AddSingleton(_mockAuthState);
        Services.AddSingleton(_mockSnackbar);
        Services.AddSingleton<IValidator<LoginRequest>>(new LoginRequestValidator());
    }

    [Fact]
    public void Login_ShouldRenderLoginForm()
    {
        // Arrange & Act
        var cut = RenderComponent<Login>();

        // Assert
        var heading = cut.Find(".mud-typography-h5");
        heading.TextContent.Should().Be("Login");
    }

    [Fact]
    public void Login_ShouldRenderEmailAndPasswordFields()
    {
        // Arrange & Act
        var cut = RenderComponent<Login>();

        // Assert
        var textFields = cut.FindComponents<MudTextField<string>>();
        textFields.Should().HaveCountGreaterOrEqualTo(2);
        
        var emailField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Email");
        var passwordField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Password");
        
        emailField.Should().NotBeNull();
        passwordField.Should().NotBeNull();
        passwordField!.Instance.InputType.Should().Be(InputType.Password);
    }

    [Fact]
    public void Login_ShouldRenderSubmitButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Login>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.Should().NotBeNull();
        submitButton.TextContent.Should().Contain("Login");
    }

    [Fact]
    public void Login_ShouldRenderRegisterLink()
    {
        // Arrange & Act
        var cut = RenderComponent<Login>();

        // Assert
        var registerLink = cut.FindAll("a").FirstOrDefault(a => a.GetAttribute("href") == "/register");
        registerLink.Should().NotBeNull();
    }

    [Fact]
    public void Login_ShouldDispatchLoginActionOnValidSubmit()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act - Fill in the form with valid data
        var inputs = cut.FindAll("input");
        var emailInput = inputs.FirstOrDefault(i => i.GetAttribute("type") != "password");
        var passwordInput = inputs.FirstOrDefault(i => i.GetAttribute("type") == "password");
        
        emailInput!.Change("test@example.com");
        passwordInput!.Change("Password123!");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<LoginAction>());
    }

    [Fact]
    public void Login_ShouldShowLoadingStateWhenAuthenticating()
    {
        // Arrange
        _mockAuthState.Value.Returns(new AuthState(
            isLoading: true,
            isAuthenticated: false,
            username: null,
            token: null,
            error: null
        ));

        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        progressCircular.Should().NotBeNull();
        
        var submitButton = cut.Find("button[type='submit']");
        submitButton.TextContent.Should().Contain("Logging in");
    }

    [Fact]
    public void Login_ShouldDisableSubmitButtonWhenLoading()
    {
        // Arrange
        _mockAuthState.Value.Returns(new AuthState(
            isLoading: true,
            isAuthenticated: false,
            username: null,
            token: null,
            error: null
        ));

        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Login_ShouldDisplayErrorWhenAuthFails()
    {
        // Arrange
        var errorMessage = "Invalid email or password";
        _mockAuthState.Value.Returns(new AuthState(
            isLoading: false,
            isAuthenticated: false,
            username: null,
            token: null,
            error: errorMessage
        ));

        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var alert = cut.FindComponent<MudAlert>();
        alert.Should().NotBeNull();
        alert.Instance.Severity.Should().Be(MudBlazor.Severity.Error);
        
        var alertText = cut.Find(".mud-alert-message");
        alertText.TextContent.Should().Contain(errorMessage);
    }

    [Fact]
    public void Login_ShouldNotDispatchWhenEmailIsInvalid()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act - Fill with invalid email
        var inputs = cut.FindAll("input");
        var emailInput = inputs.FirstOrDefault(i => i.GetAttribute("type") != "password");
        var passwordInput = inputs.FirstOrDefault(i => i.GetAttribute("type") == "password");
        
        emailInput!.Change("invalid-email");
        passwordInput!.Change("Password123!");

        var form = cut.Find("form");
        form.Submit();

        // Assert - Should not dispatch due to validation error
        _mockDispatcher.DidNotReceive().Dispatch(Arg.Any<LoginAction>());
    }

    [Fact]
    public void Login_ShouldNotDispatchWhenPasswordIsEmpty()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act - Fill with empty password
        var inputs = cut.FindAll("input");
        var emailInput = inputs.FirstOrDefault(i => i.GetAttribute("type") != "password");
        
        emailInput!.Change("test@example.com");

        var form = cut.Find("form");
        form.Submit();

        // Assert - Should not dispatch due to validation error
        _mockDispatcher.DidNotReceive().Dispatch(Arg.Any<LoginAction>());
    }
}
