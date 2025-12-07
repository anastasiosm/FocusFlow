namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown when entity validation fails (invalid input data)
/// </summary>
public class FocusFlowValidationException : FocusFlowException
{
	public FocusFlowValidationException(string message) : base(message)
	{
	}

	public FocusFlowValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
