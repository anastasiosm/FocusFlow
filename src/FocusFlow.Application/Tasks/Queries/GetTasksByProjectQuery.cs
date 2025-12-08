using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries;

/// <summary>
/// Query to get all tasks for a specific project
/// </summary>
public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<List<TaskDto>>;
