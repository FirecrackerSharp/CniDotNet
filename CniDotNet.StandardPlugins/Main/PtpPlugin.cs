using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data.Results;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record PtpPlugin(
    object Ipam,
    bool? IpMasquerade = null,
    int? Mtu = null,
    AddCniResultDns? Dns = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null)
    : TypedPlugin("ptp", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (IpMasquerade is not null)
        {
            jsonObject["ipMasq"] = IpMasquerade;
        }

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);

        if (Dns is not null)
        {
            jsonObject["dns"] = JsonSerializer.SerializeToNode(Dns, CniRuntime.SerializerOptions);
        }
    }
}