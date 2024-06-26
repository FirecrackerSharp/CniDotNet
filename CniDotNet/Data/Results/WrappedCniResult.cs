namespace CniDotNet.Data.Results;

public sealed class WrappedCniResult<T> where T : class
{
    public T? SuccessValue { get; }
    public ErrorCniResult? ErrorValue { get; }
    internal string? RawSuccessValue { get; }

    public bool IsSuccess => SuccessValue is not null;
    public bool IsError => ErrorValue is not null;
    
    private WrappedCniResult(T? successValue, ErrorCniResult? errorValue, string? rawSuccessValue)
    {
        SuccessValue = successValue;
        ErrorValue = errorValue;
        RawSuccessValue = rawSuccessValue;
    }

    internal static WrappedCniResult<T> Success(T successValue, string? rawSuccessValue = null) =>
        new(successValue, null, rawSuccessValue);

    internal static WrappedCniResult<T> Error(ErrorCniResult errorValue) =>
        new(null, errorValue, null);
}