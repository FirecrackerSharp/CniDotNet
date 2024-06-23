namespace CniDotNet.Host;

public interface ICniHostProcess
{
    string CurrentOutput { get; }
    
    Task WriteAsync(string line, CancellationToken cancellationToken);

    Task WaitForExitAsync(CancellationToken cancellationToken);
}