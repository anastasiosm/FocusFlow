namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action
/// </summary>
public class FocusFlowUnauthorizedException : FocusFlowException
{
	public FocusFlowUnauthorizedException(string message) : base(message)
	{
	}

	public FocusFlowUnauthorizedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
