using FocusFlow.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FocusFlow.Infrastructure.Identity;

/// <summary>
/// Application user extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser, IApplicationUser
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public string FullName => $"{FirstName} {LastName}".Trim();
}