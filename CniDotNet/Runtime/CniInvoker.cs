using System.Text.Json;
using CniDotNet.Data;
using CniDotNet.Data.Results;

namespace CniDotNet.Runtime;

internal static class CniInvoker
{
    public static async Task<string> InvokeAsync(
        Network network,
        CniInvocationOptions cniInvocationOptions,
        string operation,
        string pluginBinary,
        AddCniResult? previousResult,
        CancellationToken cancellationToken)
    {
        var stdinJson = DerivePluginInput(network, cniInvocationOptions.CniVersion!,
            cniInvocationOptions.ContainerId, previousResult);
        var inputPath = cniInvocationOptions.InvocationOptions.CniHost.GetTempFilePath();
        await cniInvocationOptions.InvocationOptions.CniHost.WriteFileAsync(inputPath, stdinJson, cancellationToken);
        
        var environment = new Dictionary<string, string>
        {
            { Constants.Environment.Command, operation },
            { Constants.Environment.ContainerId, cniInvocationOptions.ContainerId },
            { Constants.Environment.InterfaceName, cniInvocationOptions.InterfaceName },
            { Constants.Environment.NetworkNamespace, cniInvocationOptions.NetworkNamespace }
        };
        if (cniInvocationOptions.PluginPath is not null)
        {
            environment[Constants.Environment.PluginPath] = cniInvocationOptions.PluginPath;
        }

        var process = await cniInvocationOptions.InvocationOptions.CniHost.StartProcessAsync(
            $"{pluginBinary} < {inputPath}", environment, cniInvocationOptions.InvocationOptions.ElevationPassword!,
            cniInvocationOptions.InvocationOptions.SuPath, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.CurrentOutput;
    }
    
    private static string DerivePluginInput(Network network, string cniVersion, string name,
        AddCniResult? previousResult)
    {
        var jsonNode = network.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        jsonNode[Constants.Parsing.Name] = name;
        jsonNode[Constants.Parsing.Type] = network.Type;

        if (network.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.RuntimeConfig] = network.Capabilities;
        }

        if (previousResult is not null)
        {
            jsonNode[Constants.Parsing.PreviousResult] = JsonSerializer.SerializeToNode(previousResult)!.AsObject();
        }

        return JsonSerializer.Serialize(jsonNode);
    }
}