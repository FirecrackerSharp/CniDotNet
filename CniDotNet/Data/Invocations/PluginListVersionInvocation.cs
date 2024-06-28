using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginListVersionInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public ErrorCniResult? ErrorResult { get; }
    public Plugin? ErrorCausePlugin { get; }
    
    public bool IsSuccess { get; }
    public IReadOnlyDictionary<Plugin, VersionCniResult>? SuccessVersionResults { get; }

    private PluginListVersionInvocation(ErrorCniResult? errorResult, Plugin? errorCausePlugin,
        IReadOnlyDictionary<Plugin, VersionCniResult>? successVersionResults)
    {
        IsError = errorResult is not null;
        ErrorResult = errorResult;
        ErrorCausePlugin = errorCausePlugin;

        IsSuccess = successVersionResults is not null;
        SuccessVersionResults = successVersionResults;
    }

    internal static PluginListVersionInvocation Error(ErrorCniResult errorResult, Plugin errorCausePlugin) =>
        new(errorResult, errorCausePlugin, null);

    internal static PluginListVersionInvocation Success(
        IReadOnlyDictionary<Plugin, VersionCniResult> successVersionResults) =>
        new(null, null, successVersionResults);
}