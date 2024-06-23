using CniDotNet.Data;

namespace CniDotNet;

internal static class PluginInvoker
{
    public static async Task<string> InvokeAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        CancellationToken cancellationToken)
    {
        var stdinJson = NetworkPluginParser.SaveToStringInternal(networkPlugin, runtimeOptions.CniVersion!, runtimeOptions.ContainerId);
        var environment = new Dictionary<string, string>
        {
            { Constants.Environment.Command, operation },
            { Constants.Environment.ContainerId, runtimeOptions.ContainerId },
            { Constants.Environment.InterfaceName, runtimeOptions.InterfaceName },
            { Constants.Environment.NetworkNamespace, runtimeOptions.NetworkNamespace }
        };
        if (runtimeOptions.PluginPath is not null)
        {
            environment[Constants.Environment.PluginPath] = runtimeOptions.PluginPath;
        }

        var process = await runtimeOptions.CniHost.StartProcessWithElevationAsync(
            pluginBinary, environment, runtimeOptions.ElevationPassword!, runtimeOptions.SudoPath, cancellationToken);
        return await process.WaitForExitAsync(cancellationToken);
    }
}