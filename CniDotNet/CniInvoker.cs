using System.Text.Json;
using CniDotNet.Data;

namespace CniDotNet;

internal static class CniInvoker
{
    public static async Task<string> InvokeAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(networkPlugin, runtimeOptions.CniVersion!, runtimeOptions.ContainerId);
        var inputPath = runtimeOptions.CniHost.GetTempFilePath();
        await runtimeOptions.CniHost.WriteFileAsync(inputPath, stdinJson, cancellationToken);
        
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
            $"{pluginBinary} < {inputPath}", environment, runtimeOptions.ElevationPassword!,
            runtimeOptions.SuPath, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(NetworkPlugin networkPlugin, string? cniVersion, string? name)
    {
        var jsonNode = networkPlugin.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.Type] = networkPlugin.Type;

        if (cniVersion is not null)
        {
            jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        }

        if (name is not null)
        {
            jsonNode[Constants.Parsing.Name] = name;
        }

        if (networkPlugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = networkPlugin.Capabilities;
        }

        return JsonSerializer.Serialize(jsonNode);
    }
}