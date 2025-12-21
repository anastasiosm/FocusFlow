namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class FocusFlowBusinessRuleException : FocusFlowException
{
	public FocusFlowBusinessRuleException(string message) : base(message)
	{
	}

	public FocusFlowBusinessRuleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
