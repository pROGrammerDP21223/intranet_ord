namespace backend_net.Common;

/// <summary>
/// Result pattern for better error handling without exceptions
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; private set; }
    public int? ErrorCode { get; private set; }

    protected Result(bool isSuccess, string? errorMessage = null, int? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string errorMessage, int? errorCode = null) => new(false, errorMessage, errorCode);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(T value) : base(true)
    {
        Value = value;
    }

    private Result(string errorMessage, int? errorCode = null) : base(false, errorMessage, errorCode)
    {
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string errorMessage, int? errorCode = null) => new(errorMessage, errorCode);
}

