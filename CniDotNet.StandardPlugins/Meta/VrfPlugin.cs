using System.Text.Json.Nodes;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Meta;

public sealed record VrfPlugin(
    string VrfName,
    int? RouteTable = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("vrf", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["vrfname"] = VrfName;

        if (RouteTable is not null)
        {
            jsonObject["table"] = RouteTable;
        }
    }
}