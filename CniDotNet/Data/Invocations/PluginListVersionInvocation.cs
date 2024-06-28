using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginListVersionInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    public Plugin? ErrorCausePlugin { get; }
    
    public bool IsSuccess { get; }
    public IReadOnlyDictionary<Plugin, CniVersionResult>? SuccessVersionResults { get; }

    private PluginListVersionInvocation(CniErrorResult? errorResult, Plugin? errorCausePlugin,
        IReadOnlyDictionary<Plugin, CniVersionResult>? successVersionResults)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;
        ErrorCausePlugin = errorCausePlugin;

        IsSuccess = successVersionResults is not null;
        SuccessVersionResults = successVersionResults;
    }

    internal static PluginListVersionInvocation Error(CniErrorResult errorResult, Plugin errorCausePlugin) =>
        new(errorResult, errorCausePlugin, null);

    internal static PluginListVersionInvocation Success(
        IReadOnlyDictionary<Plugin, CniVersionResult> successVersionResults) =>
        new(null, null, successVersionResults);
}