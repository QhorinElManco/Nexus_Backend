#pragma warning disable CA1000 // Do not declare static members on generic types

namespace Nexos.Transversal.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ErrorCode ErrorCode { get; }

    protected Result(bool isSuccess, string? error, ErrorCode errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success()
    {
        return new Result(true, null, ErrorCode.None);
    }

    public static Result Failure(string error, ErrorCode code = ErrorCode.UnexpectedError)
    {
        return new Result(false, error, code);
    }
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, ErrorCode errorCode)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, null, ErrorCode.None);
    }

    public static Result<T> Fail(string error, ErrorCode code = ErrorCode.UnexpectedError)
    {
        return new Result<T>(false, default, error, code);
    }

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess ? Result<TNew>.Ok(mapper(Value!)) : Result<TNew>.Fail(Error!, ErrorCode);
    }

    public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        return IsSuccess ? Result<TNew>.Ok(await mapper(Value!)) : Result<TNew>.Fail(Error!, ErrorCode);
    }

    public T GetValueOrThrow()
    {
        return IsSuccess ? Value! : throw new InvalidOperationException($"Result is failure: {Error}");
    }

    public T GetValueOrDefault(T defaultValue)
    {
        return IsSuccess ? Value! : defaultValue;
    }
}

#pragma warning restore CA1000
