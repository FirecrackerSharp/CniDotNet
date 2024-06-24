namespace CniDotNet.Runtime;

public sealed class LockHolder
{
    internal readonly SemaphoreSlim Semaphore = new(initialCount: 1, maxCount: 1);

    public bool Unlocked => Semaphore.CurrentCount > 0;

    public bool Locked => Semaphore.CurrentCount == 0;

    public async Task WaitForUnlockAsync(
        TimeSpan? pollFrequency = null,
        CancellationToken cancellationToken = default)
    {
        pollFrequency ??= TimeSpan.FromMilliseconds(5);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(pollFrequency.Value, cancellationToken);
            if (Semaphore.CurrentCount > 0) break;
        }
    }
}