using System.Text.Json;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkPluginParser
{
    internal static string SaveToStringInternal(NetworkPlugin networkPlugin, string? cniVersion, string? name)
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