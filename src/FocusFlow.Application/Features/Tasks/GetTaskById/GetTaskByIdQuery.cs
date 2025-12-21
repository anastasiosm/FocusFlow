using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto>;
