using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByUser;

/// <summary>
/// Query to get all tasks assigned to a specific user
/// </summary>
public record GetTasksByUserQuery(string UserId) : IRequest<List<TaskDto>>;
