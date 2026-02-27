namespace backend_net.Common.Results;

/// <summary>
/// Result pattern implementation for better error handling
/// Eliminates need for exceptions in business logic
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string ErrorMessage { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string errorMessage, string? errorCode = null)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(errorMessage))
            throw new InvalidOperationException("Successful result cannot have error message");
        
        if (!isSuccess && string.IsNullOrWhiteSpace(errorMessage))
            throw new InvalidOperationException("Failed result must have error message");

        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string errorMessage, string? errorCode = null) => new(false, errorMessage, errorCode);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string errorMessage, string? errorCode = null) => new(default!, false, errorMessage, errorCode);
}

public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, string errorMessage, string? errorCode = null)
        : base(isSuccess, errorMessage, errorCode)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}

