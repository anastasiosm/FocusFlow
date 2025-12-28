using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Extensions;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

public class CreateTaskEffect : Effect<CreateTaskAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<CreateTaskEffect> _logger;

    public CreateTaskEffect(IApiService apiService, ILogger<CreateTaskEffect> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public override async Task HandleAsync(CreateTaskAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Creating task for project {ProjectId}", action.ProjectId);

        try
        {
            // Create DTO from form model with proper validation using extension method
            var dto = action.FormModel.ToCreateDto(action.ProjectId);

            var result = await _apiService.CreateTaskAsync(dto);

            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully created task {TaskId}", result.Data!.Id);
                dispatcher.Dispatch(new CreateTaskSuccessAction(result.Data!));
            }
            else
            {
                _logger.LogError("Failed to create task: {Error}", result.Error);
                dispatcher.Dispatch(new CreateTaskFailureAction(
                    result.Error ?? "Failed to create task. Please try again."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating task for project {ProjectId}", action.ProjectId);
            dispatcher.Dispatch(new CreateTaskFailureAction(
                "An unexpected error occurred. Please try again."));
        }
    }
}
