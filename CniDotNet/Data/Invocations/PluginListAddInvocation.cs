using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginListAddInvocation
{
    public bool IsError { get; }
    public ErrorCniResult? ErrorResult { get; }
    public Plugin? ErrorCausePlugin { get; }
    
    public bool IsSuccess { get; }
    public IReadOnlyList<Attachment>? SuccessAttachments { get; }
    public AddCniResult? SuccessAddResult { get; }

    private PluginListAddInvocation(ErrorCniResult? errorResult, Plugin? errorCausePlugin,
        IReadOnlyList<Attachment>? successAttachments, AddCniResult? successAddResult)
    {
        ErrorResult = errorResult;
        ErrorCausePlugin = errorCausePlugin;
        IsError = errorResult is not null;
        
        SuccessAttachments = successAttachments;
        SuccessAddResult = successAddResult;
        IsSuccess = successAttachments is not null;
    }

    internal static PluginListAddInvocation Error(ErrorCniResult errorResult, Plugin errorCausePlugin) =>
        new(errorResult, errorCausePlugin, null, null);

    internal static PluginListAddInvocation Success(IReadOnlyList<Attachment> successAttachments, AddCniResult successCniResult) =>
        new(null, null, successAttachments, successCniResult);
}