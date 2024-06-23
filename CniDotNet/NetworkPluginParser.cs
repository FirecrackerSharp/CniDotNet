using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkPluginParser
{
    internal static string SaveToStringInternal(NetworkPlugin networkPlugin, string? cniVersion, string? name)
    {
        var jsonNode = networkPlugin.PluginParameters.DeepClone();
        //jsonNode[Constants.Parsing.Type] = networkPlugin.Type;

        if (cniVersion is not null)
        {
            jsonNode[Constants.Parsing.CniVersion] = cniVersion;
        }

        if (name is not null)
        {
            jsonNode[Constants.Parsing.Name] = name;
        }

        jsonNode["runtimeConfig"] = new JsonObject();

        return JsonSerializer.Serialize(jsonNode);
    }
}