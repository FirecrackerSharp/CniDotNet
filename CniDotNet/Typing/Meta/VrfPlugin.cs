using System.Text.Json.Nodes;

namespace CniDotNet.Typing.Meta;

public record VrfPlugin(
    string VrfName,
    int? RouteTable = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null) : TypedPlugin("vrf", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["vrfname"] = VrfName;

        if (RouteTable is not null)
        {
            jsonObject["table"] = RouteTable;
        }
    }
}