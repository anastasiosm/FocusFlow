using MediatR;

namespace FocusFlow.Application.Projects.Commands;

public record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<bool>;
