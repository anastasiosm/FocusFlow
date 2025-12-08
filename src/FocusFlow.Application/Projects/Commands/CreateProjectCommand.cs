using FocusFlow.Application.DTOs;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Projects.Commands;

/// <summary>
/// Command to create a new project
/// </summary>
public record CreateProjectCommand(string Name, string? Description, string OwnerId)
	: IRequest<ProjectDto>;
