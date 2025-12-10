using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Projects.Queries;

/// <summary>
/// Query to get a project by ID
/// </summary>
public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDetailDto>;
