namespace CniDotNet.Host;

public interface ICniHostProcess
{
    string CurrentOutput { get; }

    Task WaitForExitAsync(CancellationToken cancellationToken);
}