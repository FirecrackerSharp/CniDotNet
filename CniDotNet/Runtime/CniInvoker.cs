using System.Text;
using System.Text.Json;
using CniDotNet.Data;
using CniDotNet.Data.Results;

namespace CniDotNet.Runtime;

internal static class CniInvoker
{
    public static async Task<string> InvokeAsync(
        NetworkPlugin networkPlugin,
        RuntimeOptions runtimeOptions,
        string operation,
        string pluginBinary,
        AddCniResult? previousResult,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(networkPlugin, runtimeOptions.CniVersion!,
            runtimeOptions.ContainerId, previousResult);
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

        if (runtimeOptions.Arguments.Count > 0)
        {
            var stringBuilder = new StringBuilder();

            foreach (var (key, value) in runtimeOptions.Arguments)
            {
                stringBuilder.Append($"{key}={value};");
            }

            environment[Constants.Environment.Arguments] = stringBuilder.ToString().TrimEnd(';');
        }

        var process = await runtimeOptions.CniHost.StartProcessWithElevationAsync(
            $"{pluginBinary} < {inputPath}", environment, runtimeOptions.ElevationPassword!,
            runtimeOptions.SuPath, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(NetworkPlugin networkPlugin, string cniVersion, string name,
        AddCniResult? previousResult)
    {
        var jsonNode = networkPlugin.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        jsonNode[Constants.Parsing.Name] = name;
        jsonNode[Constants.Parsing.Type] = networkPlugin.Type;

        if (networkPlugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = networkPlugin.Capabilities;
        }

        if (previousResult is not null)
        {
            jsonNode[Constants.Parsing.PreviousResult] = JsonSerializer.SerializeToNode(previousResult)!.AsObject();
        }

        return JsonSerializer.Serialize(jsonNode);
    }
}