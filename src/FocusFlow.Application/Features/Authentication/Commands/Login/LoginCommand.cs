using FocusFlow.Application.Features.Authentication.Common;
using MediatR;

namespace FocusFlow.Application.Features.Authentication.Commands.Login;

public record LoginCommand(
    string Email, 
    string Password,
    string? CorrelationId = null
) : IRequest<AuthResponse>;