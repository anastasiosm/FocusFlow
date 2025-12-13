using Microsoft.AspNetCore.Authorization;

namespace FocusFlow.WebApi.Authorization.ProjectOwnership;

/// <summary>
/// Authorization requirement that validates project ownership
/// </summary>
public class ProjectOwnershipRequirement : IAuthorizationRequirement
{
}