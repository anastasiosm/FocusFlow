namespace FocusFlow.Application.Common;

/// <summary>
/// Represents the result of an operation.
/// Implements the Result pattern for handling success and failure cases.
/// TODO: Will consider this again to see if it's necessary!
/// </summary>
public class Result
{
	public bool IsSuccess { get; }
	public string? Error { get; }

	protected Result(bool isSuccess, string? error)
	{
		IsSuccess = isSuccess;
		Error = error;
	}

	public static Result Success() => new(true, null);
	public static Result Failure(string error) => new(false, error);

	public static Result<T> Success<T>(T value) => new(value, true, null);
	public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
	public T? Value { get; }

	internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
	{
		Value = value;
	}
}