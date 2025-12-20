using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.BlazorApp.Models;
using FocusFlow.Domain.Enums;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface ITasksApi
{
    [Get("/api/projects/{projectId}/tasks")]
    Task<List<TaskDto>> GetTasksAsync(Guid projectId);

    [Get("/api/tasks")]
    Task<List<TaskDto>> GetTasksFilteredAsync(
        [Query] ProjectTaskStatus? status = null, 
        [Query] Priority? priority = null, 
        [Query] bool? isOverdue = null);

    [Get("/api/tasks/{id}")]
    Task<TaskDto> GetTaskByIdAsync(Guid id);

    [Post("/api/tasks")]
    Task<TaskDto> CreateTaskAsync([Body] CreateTaskDto dto);

    [Put("/api/tasks/{id}")]
    Task<TaskDto> UpdateTaskAsync(Guid id, [Body] UpdateTaskRequest dto);

    [Patch("/api/tasks/{id}/status")]
    Task UpdateTaskStatusAsync(Guid id, [Body] UpdateTaskStatusRequest request);

    [Delete("/api/tasks/{id}")]
    Task DeleteTaskAsync(Guid id);
}