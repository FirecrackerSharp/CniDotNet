namespace CniDotNet.Abstractions;

public interface IRuntimeHostProcess
{
    string CurrentOutput { get; }

    Task WaitForExitAsync(CancellationToken cancellationToken);
}