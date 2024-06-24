using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data.Results;
using CniDotNet.Runtime;

namespace CniDotNet.Typing;

public sealed record PtpPlugin(
    object Ipam,
    bool? IpMasq = null,
    int? Mtu = null,
    AddCniResultDns? Dns = null,
    JsonObject? Capabilities = null)
    : TypedPlugin("ptp", Capabilities)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (IpMasq.HasValue)
        {
            jsonObject["ipMasq"] = IpMasq.Value;
        }

        if (Mtu.HasValue)
        {
            jsonObject["mtu"] = Mtu.Value;
        }

        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);

        if (Dns is not null)
        {
            jsonObject["dns"] = JsonSerializer.SerializeToNode(Dns, CniRuntime.SerializerOptions);
        }
    }
}