using FocusFlow.Application.Features.Projects.Common;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetAllProjects;

/// <summary>
/// Query to get all projects
/// </summary>
public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;
