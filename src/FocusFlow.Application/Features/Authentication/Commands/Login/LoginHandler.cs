using FocusFlow.Application.Features.Authentication.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Authentication.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginHandler(
        IUserService userService,
        IJwtTokenGenerator tokenGenerator)
    {
        _userService = userService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new FocusFlowUnauthorizedException("Invalid email or password");
        }

        var isPasswordValid = await _userService.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new FocusFlowUnauthorizedException("Invalid email or password");
        }

        var token = await _tokenGenerator.GenerateAsync(user);

        return new AuthResponse
        {
            Token = token,
            UserName = user.FullName,
            Email = user.Email!,
            Expiration = DateTime.UtcNow.AddHours(24) // TODO: Make configurable
        };
    }
}