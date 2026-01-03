using FocusFlow.Application.Features.Authentication.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Authentication.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RegisterHandler(
        IUserService userService,
        IJwtTokenGenerator tokenGenerator)
    {
        _userService = userService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userService.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new FocusFlowValidationException("User with this email already exists");
        }

        // Create new user
        var (succeeded, errors) = await _userService.CreateUserAsync(request.Email, request.Password, request.FirstName, request.LastName);

        if (!succeeded)
        {
            var errorMessages = string.Join(", ", errors);
            throw new FocusFlowValidationException($"Registration failed: {errorMessages}");
        }

        // Get the created user and generate token
        var user = await _userService.FindByEmailAsync(request.Email);
        var token = await _tokenGenerator.GenerateAsync(user!);

        return new AuthResponse
        {
            Token = token,
            UserName = user!.FullName,
            Email = user.Email!,
            Expiration = DateTime.UtcNow.AddHours(24) // TODO: Make configurable
        };
    }
}