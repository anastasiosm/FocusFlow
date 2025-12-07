namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class FocusFlowNotFoundException : FocusFlowException
{
	public string EntityName { get; }
	public object EntityId { get; }

	public FocusFlowNotFoundException(string entityName, object entityId)
		: base($"{entityName} with ID '{entityId}' was not found.")
	{
		EntityName = entityName;
		EntityId = entityId;
	}

	public FocusFlowNotFoundException(string entityName, object entityId, Exception innerException)
		: base($"{entityName} with ID '{entityId}' was not found.", innerException)
	{
		EntityName = entityName;
		EntityId = entityId;
	}
}
