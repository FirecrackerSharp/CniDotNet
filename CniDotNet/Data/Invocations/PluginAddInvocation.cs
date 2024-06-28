using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginAddInvocation : IBaseInvocation
{
    public bool IsError { get; }
    public CniErrorResult? ErrorResult { get; }
    
    public bool IsSuccess { get; }
    public Attachment? SuccessAttachment { get; }
    public CniAddResult? SuccessAddResult { get; }

    private PluginAddInvocation(CniErrorResult? errorResult, Attachment? successAttachment,
        CniAddResult? successAddResult)
    {
        ErrorResult = errorResult;
        IsError = errorResult is not null;

        SuccessAttachment = successAttachment;
        SuccessAddResult = successAddResult;
        IsSuccess = successAttachment is not null;
    }

    internal static PluginAddInvocation Error(CniErrorResult errorResult) =>
        new(errorResult, null, null);

    internal static PluginAddInvocation Success(Attachment successAttachment, CniAddResult successAddResult) =>
        new(null, successAttachment, successAddResult);
}