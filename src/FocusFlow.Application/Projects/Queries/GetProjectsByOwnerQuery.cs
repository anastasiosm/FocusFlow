using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Projects.Queries;

/// <summary>
/// Query to get projects by owner
/// </summary>
public record GetProjectsByOwnerQuery(string OwnerId) : IRequest<List<ProjectDto>>;
