using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Application.Features.Tasks.Common;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public class CreateTaskEffect : Effect<CreateTaskAction>
{
    private readonly IApiService _apiService;

    public CreateTaskEffect(IApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task HandleAsync(CreateTaskAction action, IDispatcher dispatcher)
    {
        try
        {
            var dto = new CreateTaskDto(
                action.Command.ProjectId,
                action.Command.Title,
                action.Command.Description,
                action.Command.DueDate ?? DateTime.UtcNow, // Or handle null appropriately
                action.Command.Priority,
                action.Command.AssignedUserId
            );

            var result = await _apiService.CreateTaskAsync(dto);

            if (result.Succeeded)
            {
                dispatcher.Dispatch(new CreateTaskSuccessAction(result.Data!));
            }
            else
            {
                dispatcher.Dispatch(new CreateTaskFailureAction(result.Error ?? "Unknown error"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreateTaskFailureAction(ex.Message));
        }
    }
}
