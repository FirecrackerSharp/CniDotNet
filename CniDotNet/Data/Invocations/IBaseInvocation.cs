namespace CniDotNet.Data.Invocations;

public interface IBaseInvocation
{
    bool IsError { get; }
    bool IsSuccess { get; }
}