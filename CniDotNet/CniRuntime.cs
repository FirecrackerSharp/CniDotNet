using System.Text.Json;
using CniDotNet.Data;
using CniDotNet.Data.Results;
using CniDotNet.Host;

namespace CniDotNet;

public static class CniRuntime
{
    private const string OperationAdd = "ADD";
    
    public static async Task<AddResult> AddPluginAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        PluginLookupOptions? pluginLookupOptions = null,
        CancellationToken cancellationToken = default)
    {
        var pluginBinary = LookupPluginBinary(runtimeOptions.CniHost, networkPlugin, pluginLookupOptions);
        var addResultJson = await PluginInvoker.InvokeAsync(networkPlugin, runtimeOptions, OperationAdd, pluginBinary!, cancellationToken);
        return JsonSerializer.Deserialize<AddResult>(addResultJson) ?? throw new JsonException("Could not deserialize AddResult");
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