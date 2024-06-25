using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data.Results;
using CniDotNet.Runtime;

namespace CniDotNet.Typing.Main;

public sealed record VlanPlugin(
    string Master,
    uint VlanId,
    object Ipam,
    uint? Mtu = null,
    AddCniResultDns? Dns = null,
    bool? LinkInContainer = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null)
    : TypedPlugin("vlan", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
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