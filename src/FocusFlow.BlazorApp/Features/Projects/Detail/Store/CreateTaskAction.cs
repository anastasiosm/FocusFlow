using FocusFlow.BlazorApp.Models.Dtos;
using FocusFlow.BlazorApp.Models;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

/// <summary>
/// Triggers task creation within a project
/// </summary>
public record CreateTaskAction(Guid ProjectId, CreateTaskFormModel FormModel);

/// <summary>
/// Dispatched when task is successfully created
/// </summary>
public record CreateTaskSuccessAction(TaskDto Task);

/// <summary>
/// Dispatched when task creation fails (validation or API error)
/// </summary>
public record CreateTaskFailureAction(string ErrorMessage);
