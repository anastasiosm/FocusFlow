using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByProject;

/// <summary>
/// Query to get all tasks for a specific project
/// </summary>
public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<List<TaskDto>>;
