using FocusFlow.Application.Features.Projects.GetProjectById;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public record LoadProjectDetailAction(Guid ProjectId);
public record LoadProjectDetailSuccessAction(ProjectDetailDto Project);
public record LoadProjectDetailFailureAction(string Error);
