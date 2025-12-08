using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries;

/// <summary>
/// Query to get all tasks assigned to a specific user
/// </summary>
public record GetTasksByUserQuery(string UserId) : IRequest<List<TaskDto>>;
