using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Projects.Queries;

/// <summary>
/// Query to get all projects
/// </summary>
public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;
