using Fluxor;
using FocusFlow.BlazorApp.Features.Tasks.Edit.Extensions;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Features.Tasks.List.Store;

/// <summary>
/// Effects for Tasks feature
/// </summary>
public class TasksListEffects
{
    private readonly IApiService _apiService;
    private readonly ILogger<TasksListEffects> _logger;
    private readonly IState<TasksListState> _state;

    public TasksListEffects(IApiService apiService, ILogger<TasksListEffects> logger, IState<TasksListState> state)
    {
        _apiService = apiService;
        _logger = logger;
        _state = state;
    }

    [EffectMethod]
    public async Task HandleLoadTasks(TasksListActions.LoadTasksAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Loading tasks with filters...");

        // Get filters from current state
        var status = _state.Value.StatusFilter;
        var priority = _state.Value.PriorityFilter;
        var isOverdue = _state.Value.IsOverdueFilter;

        var result = await _apiService.GetTasksFilteredAsync(status, priority, isOverdue);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully loaded {Count} tasks", result.Data?.Count ?? 0);
            dispatcher.Dispatch(new TasksListActions.LoadTasksSuccessAction(result.Data ?? new()));
        }
        else
        {
            _logger.LogError("Failed to load tasks: {Error}", result.Error);
            dispatcher.Dispatch(new TasksListActions.LoadTasksFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleSetStatusFilter(TasksListActions.SetStatusFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying status filter: {Status}", action.Status);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetPriorityFilter(TasksListActions.SetPriorityFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying priority filter: {Priority}", action.Priority);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetOverdueFilter(TasksListActions.SetOverdueFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying overdue filter: {IsOverdue}", action.IsOverdue);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleClearFilters(TasksListActions.ClearFiltersAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Clearing all filters");
        
        // Filters cleared in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    // Task Detail effects
    [EffectMethod]
    public async Task HandleLoadTaskById(TasksListActions.LoadTaskByIdAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Loading task by ID: {TaskId}", action.TaskId);

        var result = await _apiService.GetTaskByIdAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully loaded task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to load task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdFailureAction(result.Error ?? "Unknown error"));
        }
    }
  
    [EffectMethod]
    public async Task HandleUpdateTaskFromResult(TasksListActions.UpdateTaskFromResultAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task from edit result: {TaskId}", action.TaskId);

        // Convert TaskEditResult to UpdateTaskRequest using extension method
        var updateRequest = action.EditResult.ToUpdateRequest();
        var result = await _apiService.UpdateTaskAsync(action.TaskId, updateRequest);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to update task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }    

    [EffectMethod]
    public async Task HandleUpdateTaskStatus(TasksListActions.UpdateTaskStatusAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task status: {TaskId} to {Status}", action.TaskId, action.Status);

        var result = await _apiService.UpdateTaskStatusAsync(action.TaskId, action.Status);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task status: {TaskId}", action.TaskId);
            // Reload the task to get updated data
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to update task status {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskStatusFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteTask(TasksListActions.DeleteTaskAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Deleting task: {TaskId}", action.TaskId);

        var result = await _apiService.DeleteTaskAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully deleted task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.DeleteTaskSuccessAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to delete task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.DeleteTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }

    // ============================================================================
    // NEW: SignalR Effects - Handle External Events
    // ============================================================================

    /// <summary>
    /// When SignalR notifies us about a task creation, fetch the full task data
    /// and add it to our list if it matches current filters.
    /// </summary>
    [EffectMethod]
    public async Task HandleTaskCreatedFromSignalR(TasksListActions.TaskCreatedFromSignalRAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("🔔 Handling SignalR TaskCreated | TaskId: {TaskId}", action.TaskId);

        try
        {
            // Fetch the full task data from API
            var result = await _apiService.GetTaskByIdAsync(action.TaskId);

            if (result.Succeeded && result.Data != null)
            {
                // Check if task matches current filters before adding
                var task = result.Data;
                _logger.LogInformation("🔍 Task fetched from API | TaskId: {TaskId} | Title: {Title}", task.Id, task.Title);
                
                if (TaskMatchesCurrentFilters(task))
                {
                    _logger.LogInformation("✅ Task matches filters, dispatching AddTaskToListAction | TaskId: {TaskId}", task.Id);
                    dispatcher.Dispatch(new TasksListActions.AddTaskToListAction(task));
                    _logger.LogInformation("✅ Added new task to list from SignalR");
                }
                else
                {
                    _logger.LogInformation("ℹ️ Task doesn't match current filters, not adding to list");
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to fetch task data for SignalR created task: {TaskId}", action.TaskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling SignalR TaskCreated for task: {TaskId}", action.TaskId);
        }
    }

    /// <summary>
    /// When SignalR notifies us about a task update, fetch the latest data
    /// and update our list.
    /// </summary>
    [EffectMethod]
    public async Task HandleTaskUpdatedFromSignalR(TasksListActions.TaskUpdatedFromSignalRAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("🔔 Handling SignalR TaskUpdated | TaskId: {TaskId}", action.TaskId);

        try
        {
            // Fetch the updated task data from API
            var result = await _apiService.GetTaskByIdAsync(action.TaskId);

            if (result.Succeeded && result.Data != null)
            {
                var task = result.Data;
                
                // Check if task still matches current filters
                if (TaskMatchesCurrentFilters(task))
                {
                    dispatcher.Dispatch(new TasksListActions.UpdateTaskInListAction(task));
                    _logger.LogInformation("✅ Updated task in list from SignalR");
                }
                else
                {
                    // Task no longer matches filters, remove it
                    dispatcher.Dispatch(new TasksListActions.RemoveTaskFromListAction(action.TaskId));
                    _logger.LogInformation("ℹ️ Task no longer matches filters, removed from list");
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to fetch updated task data for SignalR: {TaskId}", action.TaskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling SignalR TaskUpdated for task: {TaskId}", action.TaskId);
        }
    }

    /// <summary>
    /// When SignalR notifies us about a status change, fetch the latest data
    /// and update our list.
    /// </summary>
    [EffectMethod]
    public async Task HandleTaskStatusChangedFromSignalR(TasksListActions.TaskStatusChangedFromSignalRAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("🔔 Handling SignalR TaskStatusChanged | TaskId: {TaskId} | Status: {Status}", 
            action.TaskId, action.NewStatus);

        try
        {
            // Fetch the updated task data from API
            var result = await _apiService.GetTaskByIdAsync(action.TaskId);

            if (result.Succeeded && result.Data != null)
            {
                var task = result.Data;
                
                // Check if task still matches current filters
                if (TaskMatchesCurrentFilters(task))
                {
                    dispatcher.Dispatch(new TasksListActions.UpdateTaskInListAction(task));
                    _logger.LogInformation("✅ Updated task status in list from SignalR");
                }
                else
                {
                    // Task no longer matches filters, remove it
                    dispatcher.Dispatch(new TasksListActions.RemoveTaskFromListAction(action.TaskId));
                    _logger.LogInformation("ℹ️ Task no longer matches filters after status change, removed from list");
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to fetch updated task data for SignalR status change: {TaskId}", action.TaskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling SignalR TaskStatusChanged for task: {TaskId}", action.TaskId);
        }
    }

    /// <summary>
    /// When SignalR notifies us about a task deletion, remove it from our list.
    /// </summary>
    [EffectMethod]
    public async Task HandleTaskDeletedFromSignalR(TasksListActions.TaskDeletedFromSignalRAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("🔔 Handling SignalR TaskDeleted | TaskId: {TaskId}", action.TaskId);

        try
        {
            dispatcher.Dispatch(new TasksListActions.RemoveTaskFromListAction(action.TaskId));
            _logger.LogInformation("✅ Removed deleted task from list via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling SignalR TaskDeleted for task: {TaskId}", action.TaskId);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Helper method to check if a task matches the current filters.
    /// This prevents adding/keeping tasks that don't belong in the current view.
    /// </summary>
    private bool TaskMatchesCurrentFilters(FocusFlow.BlazorApp.Features.Tasks.Shared.Models.TaskResponse task)
    {
        var state = _state.Value;

        // Check status filter
        if (state.StatusFilter.HasValue && task.Status != state.StatusFilter.Value)
            return false;

        // Check priority filter
        if (state.PriorityFilter.HasValue && task.Priority != state.PriorityFilter.Value)
            return false;

        // Check overdue filter
        if (state.IsOverdueFilter.HasValue)
        {
            var isOverdue = task.DueDate.HasValue && 
                           task.DueDate.Value < DateTime.UtcNow && 
                           task.Status != ProjectTaskStatus.Done;
            
            if (state.IsOverdueFilter.Value != isOverdue)
                return false;
        }

        return true;
    }
}
