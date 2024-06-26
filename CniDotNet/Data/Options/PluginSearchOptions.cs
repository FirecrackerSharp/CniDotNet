using CniDotNet.Abstractions;

namespace CniDotNet.Data.Options;

public sealed record PluginSearchOptions(
    IReadOnlyDictionary<string, string>? SearchTable = null,
    string? Directory = null,
    string EnvironmentVariable = "PLUGIN_PATH",
    SearchOption DirectorySearchOption = SearchOption.TopDirectoryOnly)
{
    internal string? CachedActualDirectory { get; private set; }
    
    internal async Task<string?> GetActualDirectoryAsync(IRuntimeHost runtimeHost, CancellationToken cancellationToken)
    {
        var directory = Directory;
        if (directory is not null)
        {
            CachedActualDirectory = directory;
            return directory;
        }
        
        directory = await runtimeHost.GetEnvironmentVariableAsync(EnvironmentVariable, cancellationToken);
        CachedActualDirectory = directory;
        return directory;
    }
}