using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginVersionInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public ErrorCniResult? ErrorResult { get; }
    
    public bool IsSuccess { get; }
    public VersionCniResult? SuccessVersionResult { get; }

    private PluginVersionInvocation(ErrorCniResult? errorResult, VersionCniResult? successVersionResult)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;

        IsSuccess = successVersionResult is not null;
        SuccessVersionResult = successVersionResult;
    }

    internal static PluginVersionInvocation Error(ErrorCniResult errorResult) =>
        new(errorResult, null);

    internal static PluginVersionInvocation Success(VersionCniResult successVersionResult) =>
        new(null, successVersionResult);
}