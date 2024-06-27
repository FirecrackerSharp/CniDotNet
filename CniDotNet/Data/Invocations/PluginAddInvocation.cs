using CniDotNet.Data.CniResults;

namespace CniDotNet.Data.Invocations;

public sealed class PluginAddInvocation
{
    public bool IsError { get; }
    public ErrorCniResult? ErrorResult { get; }
    
    public bool IsSuccess { get; }
    public Attachment? SuccessAttachment { get; }
    public AddCniResult? SuccessAddResult { get; }

    private PluginAddInvocation(ErrorCniResult? errorResult, Attachment? successAttachment,
        AddCniResult? successAddResult)
    {
        ErrorResult = errorResult;
        IsError = errorResult is not null;

        SuccessAttachment = successAttachment;
        SuccessAddResult = successAddResult;
        IsSuccess = successAttachment is not null;
    }

    internal static PluginAddInvocation Error(ErrorCniResult errorResult) =>
        new(errorResult, null, null);

    internal static PluginAddInvocation Success(Attachment successAttachment, AddCniResult successAddResult) =>
        new(null, successAttachment, successAddResult);
}