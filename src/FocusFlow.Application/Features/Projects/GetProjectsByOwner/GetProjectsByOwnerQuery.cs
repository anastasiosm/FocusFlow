using FocusFlow.Application.Features.Projects.Common;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetProjectsByOwner;

/// <summary>
/// Query to get projects by owner
/// </summary>
public record GetProjectsByOwnerQuery(string OwnerId) : IRequest<List<ProjectDto>>;
