using FocusFlow.BlazorApp.Models;
using Refit;
using FocusFlow.Domain.Enums;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;
using FocusFlow.BlazorApp.Features.Tasks.Create.Models;
using FocusFlow.BlazorApp.Features.Tasks.UpdateStatus.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.Shared.Services;

public interface ITasksApi
{
    [Get("/api/projects/{projectId}/tasks")]
    Task<List<TaskResponse>> GetTasksAsync(Guid projectId);

    [Get("/api/tasks")]
    Task<List<TaskResponse>> GetTasksFilteredAsync(
        [Query] ProjectTaskStatus? status = null, 
        [Query] Priority? priority = null, 
        [Query] bool? isOverdue = null);

    [Get("/api/tasks/{id}")]
    Task<TaskResponse> GetTaskByIdAsync(Guid id);

    [Post("/api/tasks")]
    Task<TaskResponse> CreateTaskAsync([Body] CreateTaskRequest dto);

    [Put("/api/tasks/{id}")]
    Task<TaskResponse> UpdateTaskAsync(Guid id, [Body] UpdateTaskRequest dto);

    [Patch("/api/tasks/{id}/status")]
    Task UpdateTaskStatusAsync(Guid id, [Body] UpdateTaskStatusRequest request);

    [Delete("/api/tasks/{id}")]
    Task DeleteTaskAsync(Guid id);
}