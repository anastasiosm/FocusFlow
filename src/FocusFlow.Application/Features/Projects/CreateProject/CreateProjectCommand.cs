using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Projects.CreateProject;

/// <summary>
/// Command to create a new project
/// </summary>
public record CreateProjectCommand(
    string Name, 
    string? Description, 
    string OwnerId,
    string? CorrelationId = null
) : IRequest<ProjectDto>;
