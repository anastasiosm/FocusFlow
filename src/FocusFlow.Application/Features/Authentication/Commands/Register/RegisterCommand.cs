using FocusFlow.Application.Features.Authentication.Common;
using MediatR;

namespace FocusFlow.Application.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string Email, 
    string Password, 
    string? FirstName = null,
    string? LastName = null,
    string? CorrelationId = null
) : IRequest<AuthResponse>;