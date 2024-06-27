using Renci.SshNet;

namespace CniDotNet.Abstractions;

internal sealed class SshRuntimeHostProcess(SshCommand sshCommand, IAsyncResult asyncResult) : IRuntimeHostProcess
{
    private string? _currentOutput;

    public string CurrentOutput => _currentOutput!;
    
    public async Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            if (asyncResult.IsCompleted) break;
        }

        _currentOutput = sshCommand.EndExecute(asyncResult);
    }
}