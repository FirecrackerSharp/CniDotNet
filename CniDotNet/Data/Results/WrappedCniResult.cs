using CniDotNet.Data.Results.Error;

namespace CniDotNet.Data.Results;

public sealed class WrappedCniResult<T> where T : class
{
    public T? SuccessValue { get; }
    public ErrorCniResult? ErrorValue { get; }
    
    private WrappedCniResult(T? successValue, ErrorCniResult? errorValue)
    {
        SuccessValue = successValue;
        ErrorValue = errorValue;
    }

    internal static WrappedCniResult<T> Success(T successValue) => new(successValue, null);

    internal static WrappedCniResult<T> Error(ErrorCniResult errorValue) => new(null, errorValue);
}