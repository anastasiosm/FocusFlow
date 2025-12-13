using System;

namespace FocusFlow.BlazorApp.Models;

public class ProjectUpdateFormModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
