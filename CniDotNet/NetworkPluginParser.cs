using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkPluginParser
{
    public static string SaveToString(NetworkPlugin networkPlugin)
        => SaveToStringInternal(networkPlugin, null);

    internal static string SaveToStringInternal(NetworkPlugin networkPlugin, string? cniVersion)
    {
        var jsonObj = new JsonObject();
        
        jsonObj[ParsingConstants.Type] = JsonValue.Create(networkPlugin.Type);
        
        if (networkPlugin.Capabilities is not null)
        {
            jsonObj[ParsingConstants.Capabilities] = networkPlugin.Capabilities;
        }
        
        foreach (var (key, value) in networkPlugin.PluginParameters)
        {
            if (value is null) continue;
            
            if (value.GetValueKind() == JsonValueKind.String)
            {
                jsonObj[key] = value.AsValue().GetValue<string>();
            }
            else if (value.GetValueKind() == JsonValueKind.Object)
            {
                jsonObj[key] = value.AsObject();
            }
        }

        if (cniVersion is not null)
        {
            jsonObj[ParsingConstants.CniVersion] = cniVersion;
        }

        return JsonSerializer.Serialize(jsonObj);
    }
}