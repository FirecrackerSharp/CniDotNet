using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginVersionInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    
    public bool IsSuccess { get; }
    public CniVersionResult? SuccessVersionResult { get; }

    private PluginVersionInvocation(CniErrorResult? errorResult, CniVersionResult? successVersionResult)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;

        IsSuccess = successVersionResult is not null;
        SuccessVersionResult = successVersionResult;
    }

    internal static PluginVersionInvocation Error(CniErrorResult errorResult) =>
        new(errorResult, null);

    internal static PluginVersionInvocation Success(CniVersionResult successVersionResult) =>
        new(null, successVersionResult);
}