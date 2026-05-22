namespace RetailOps.Shared.Kernel.Domain.Results;

public class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };

    public string Error => Errors.FirstOrDefault() ?? string.Empty;
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, string error)
    {
        _value = value;
        IsSuccess = isSuccess;
        Errors = string.IsNullOrEmpty(error) ? [] : [error];
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static Result<T> Success(T value) => new(value, true, string.Empty);

    public static new Result<T> Failure(string error) => new(default, false, error);
}
