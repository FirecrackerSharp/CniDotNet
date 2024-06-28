using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginListAddInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    public Plugin? ErrorCausePlugin { get; }
    
    public bool IsSuccess { get; }
    public IReadOnlyList<Attachment>? SuccessAttachments { get; }
    public CniAddResult? SuccessAddResult { get; }

    private PluginListAddInvocation(CniErrorResult? errorResult, Plugin? errorCausePlugin,
        IReadOnlyList<Attachment>? successAttachments, CniAddResult? successAddResult)
    {
        ErrorResult = errorResult;
        ErrorCausePlugin = errorCausePlugin;
        IsError = errorResult is not null;
        
        SuccessAttachments = successAttachments;
        SuccessAddResult = successAddResult;
        IsSuccess = successAttachments is not null;
    }

    internal static PluginListAddInvocation Error(CniErrorResult errorResult, Plugin errorCausePlugin) =>
        new(errorResult, errorCausePlugin, null, null);

    internal static PluginListAddInvocation Success(IReadOnlyList<Attachment> successAttachments, CniAddResult successCniResult) =>
        new(null, null, successAttachments, successCniResult);
}