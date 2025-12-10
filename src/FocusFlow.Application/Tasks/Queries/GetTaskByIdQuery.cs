using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries
{
	public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto>;
}
