using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data.Results;

namespace CniDotNet.Typing;

public record PtpTypedNetwork(
    object Ipam,
    bool? IpMasq = null,
    int? Mtu = null,
    AddCniResultDns? Dns = null,
    JsonObject? Capabilities = null)
    : TypedNetwork(TypedConstants.Ptp, Capabilities)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (IpMasq.HasValue)
        {
            jsonObject[TypedConstants.IpMasquerade] = IpMasq.Value;
        }

        if (Mtu.HasValue)
        {
            jsonObject[TypedConstants.Mtu] = Mtu.Value;
        }

        jsonObject[TypedConstants.Ipam] = JsonSerializer.SerializeToNode(Ipam, SerializerOptions);

        if (Dns is not null)
        {
            jsonObject[TypedConstants.Dns] = JsonSerializer.SerializeToNode(Dns, SerializerOptions);
        }
    }
}