using MediatR;

namespace FocusFlow.Application.Features.Projects.GetProjectById;

/// <summary>
/// Query to get a project by ID
/// </summary>
public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDetailDto>;
