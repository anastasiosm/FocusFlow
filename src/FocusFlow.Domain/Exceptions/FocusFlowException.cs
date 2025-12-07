namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Base exception for all FocusFlow domain-specific errors
/// </summary>
public abstract class FocusFlowException : Exception
{
	protected FocusFlowException(string message) : base(message)
	{
	}

	protected FocusFlowException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
