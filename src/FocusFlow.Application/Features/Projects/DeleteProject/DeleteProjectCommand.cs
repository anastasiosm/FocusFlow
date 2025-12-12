using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Projects.DeleteProject;

/// <summary>
/// Command to delete a project
/// </summary>
public record DeleteProjectCommand(Guid Id, string UserId) : IRequest;
