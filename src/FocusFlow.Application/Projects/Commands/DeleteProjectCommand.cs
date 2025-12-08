using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Projects.Commands;

/// <summary>
/// Command to delete a project
/// </summary>
public record DeleteProjectCommand(Guid Id) : IRequest;
