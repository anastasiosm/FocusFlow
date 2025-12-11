using MediatR;

namespace FocusFlow.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<bool>;
