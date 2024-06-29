using CniDotNet.Abstractions;

namespace CniDotNet.Tests.Helpers;

public class TestRuntimeHostProcess(string content) : IRuntimeHostProcess
{
    public string CurrentOutput { get; } = content;

    public Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}