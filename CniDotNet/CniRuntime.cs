using CniDotNet.Abstractions;
using CniDotNet.Data;

namespace CniDotNet;

public static class CniRuntime
{
    private const string OperationAdd = "ADD";
    
    public static async Task AddSinglePluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        await PluginInvoker.InvokeAsync(networkPlugin, runtimeOptions, OperationAdd, pluginBinary!, cancellationToken);
    }

    private static string? LookupPluginBinary(ICniHost cniHost,
        NetworkPlugin networkPlugin, PluginLookupOptions? pluginLookupOptions)
    {
        pluginLookupOptions ??= PluginLookupOptions.Default;
        var directory = pluginLookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(pluginLookupOptions.EnvironmentVariable);
        if (directory is null) return null;

        if (!cniHost.DirectoryExists(directory)) return null;

        var matchingFiles = cniHost.EnumerateDirectory(
            directory, networkPlugin.Type, pluginLookupOptions.DirectorySearchOption);
        return matchingFiles.FirstOrDefault();
    }
}