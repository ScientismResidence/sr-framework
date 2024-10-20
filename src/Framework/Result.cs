namespace Framework;

public readonly struct Result<TResult, TError>
{
    private Result(TResult value, TError error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }
    
    public TResult Value { get; }
    
    public TError Error { get; }
    
    public bool IsSuccess { get; }

    public static Result<TResult, TError> Success(TResult value)
    {
        return new Result<TResult, TError>(value, default, true);
    }

    public static Result<TResult, TError> Failure(TError error)
    {
        return new Result<TResult, TError>(default, error, false);
    }
}