namespace FocusFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown when entity validation fails (invalid input data)
/// </summary>
public class FocusFlowValidationException : FocusFlowException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

	public FocusFlowValidationException(string message) : base(message)
	{
        Errors = new Dictionary<string, string[]>();
	}

	public FocusFlowValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
        Errors = new Dictionary<string, string[]>();
	}

    public FocusFlowValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
