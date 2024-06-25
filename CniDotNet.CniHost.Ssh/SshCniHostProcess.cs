using CniDotNet.Host;
using Renci.SshNet;

namespace CniDotNet.CniHost.Ssh;

internal sealed class SshCniHostProcess(SshCommand sshCommand, IAsyncResult asyncResult) : ICniHostProcess
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