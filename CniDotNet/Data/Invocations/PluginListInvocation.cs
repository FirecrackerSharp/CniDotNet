using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginListInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    public Plugin? ErrorCausePlugin { get; }
    
    public bool IsSuccess { get; }

    private PluginListInvocation(CniErrorResult? errorResult, Plugin? errorCausePlugin)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;
        ErrorCausePlugin = errorCausePlugin;
        IsSuccess = errorResult is null;
    }

    internal static readonly PluginListInvocation Success = new(null, null);

    internal static PluginListInvocation Error(CniErrorResult errorResult, Plugin errorCausePlugin) =>
        new(errorResult, errorCausePlugin);
}