using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data.CniResults;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record VlanPlugin(
    string Master,
    uint VlanId,
    object Ipam,
    uint? Mtu = null,
    CniAddResultDns? Dns = null,
    bool? LinkInContainer = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null)
    : TypedPlugin("vlan", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["master"] = Master;
        jsonObject["vlanId"] = VlanId;
        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        if (Dns is not null)
        {
            jsonObject["dns"] = JsonSerializer.SerializeToNode(Dns, CniRuntime.SerializerOptions);
        }

        if (LinkInContainer is not null)
        {
            jsonObject["linkInContainer"] = LinkInContainer;
        }
    }
}