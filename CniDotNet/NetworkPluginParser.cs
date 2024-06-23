using System.Text.Json;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkPluginParser
{
    public static string SaveToString(NetworkPlugin networkPlugin)
        => SaveToStringInternal(networkPlugin, null);

    internal static string SaveToStringInternal(NetworkPlugin networkPlugin, string? cniVersion)
    {
        var jsonNode = networkPlugin.PluginParameters.DeepClone();
        jsonNode[Constants.Parsing.Type] = networkPlugin.Type;

        if (networkPlugin.Capabilities is not null)
        {
            jsonNode[Constants.Parsing.Capabilities] = networkPlugin.Capabilities;
        }

        if (cniVersion is not null)
        {
            jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        }

        return JsonSerializer.Serialize(jsonNode);
    }
}