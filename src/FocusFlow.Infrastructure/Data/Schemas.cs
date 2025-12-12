namespace FocusFlow.Infrastructure.Data;

public static class Schemas
{
	/// <summary>
	/// Application domain schema (Projects, Tasks)
	/// </summary>
	public const string Application = "focus_flow";

	/// <summary>
	/// Identity schema (Users, Roles, etc.)
	/// Uses default schema for ASP.NET Core Identity compatibility
	/// </summary>
	public const string Identity = "public"; // PostgreSQL default schema
}