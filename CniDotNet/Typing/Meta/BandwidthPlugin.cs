using System.Text.Json.Nodes;

namespace CniDotNet.Typing.Meta;

public sealed record BandwidthPlugin(
    uint IngressRate,
    uint IngressBurst,
    uint EgressRate,
    uint EgressBurst,
    JsonObject? Capabilities = null,
    JsonObject? Args = null) : TypedPlugin("bandwidth", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["ingressRate"] = IngressRate;
        jsonObject["ingressBurst"] = IngressBurst;
        jsonObject["egressRate"] = EgressRate;
        jsonObject["egressBurst"] = EgressBurst;
    }
}