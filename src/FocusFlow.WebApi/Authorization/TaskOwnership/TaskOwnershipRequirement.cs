using Microsoft.AspNetCore.Authorization;

namespace FocusFlow.WebApi.Authorization.TaskOwnership;

/// <summary>
/// Authorization requirement that validates the current user owns the project containing the task
/// </summary>
public class TaskOwnershipRequirement : IAuthorizationRequirement
{
}