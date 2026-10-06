using System.Diagnostics.CodeAnalysis;

namespace FinTech.SharedKernel;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Standard domain error pattern")]
public sealed record DomainError(string Code, string Message)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);
    public static readonly DomainError NullValue = new("General.NullValue", "A null value was provided where a value was required.");

    public static DomainError Failure(string code, string message) => new(code, message);
    public static DomainError NotFound(string code, string message) => new(code, message);
    public static DomainError Validation(string code, string message) => new(code, message);
    public static DomainError Conflict(string code, string message) => new(code, message);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainError Error { get; }

    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None)
        {
            throw new InvalidOperationException("Success result cannot have an error.");
        }

        if (!isSuccess && error == DomainError.None)
        {
            throw new InvalidOperationException("Failure result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, DomainError.None);
    public static Result Failure(DomainError error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.CreateSuccess(value);
    public static Result<TValue> Failure<TValue>(DomainError error) => Result<TValue>.CreateFailure(error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected Result(TValue? value, bool isSuccess, DomainError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    internal static Result<TValue> CreateSuccess(TValue value) => new(value, true, DomainError.None);
    internal static Result<TValue> CreateFailure(DomainError error) => new(default, false, error);

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? CreateSuccess(value) : CreateFailure(DomainError.NullValue);

    public static implicit operator Result<TValue>(DomainError error) => CreateFailure(error);
}
