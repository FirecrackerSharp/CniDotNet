using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    
    public bool IsSuccess { get; }

    private PluginInvocation(CniErrorResult? errorResult)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;
        IsSuccess = errorResult is null;
    }

    internal static readonly PluginInvocation Success = new(null);

    internal static PluginInvocation Error(CniErrorResult errorResult) => new(errorResult);
}