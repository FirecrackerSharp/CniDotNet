namespace CniDotNet.Host;

public interface ICniHostProcess
{
    Task WriteLineAsync(string line, CancellationToken cancellationToken);

    Task<string> WaitForExitAsync(CancellationToken cancellationToken);
}