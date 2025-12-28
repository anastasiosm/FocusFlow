using System;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Models;

public class ProjectUpdateFormModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
