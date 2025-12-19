using FocusFlow.Application.Features.Projects.GetProjectById;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetProjectWithTasks;

/// <summary>
/// Query to get project with full task details
/// </summary>
/// <param name="Id">Project ID</param>
public record GetProjectWithTasksQuery(Guid Id) : IRequest<ProjectDetailDto>;