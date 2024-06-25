using System.Text.Json.Nodes;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Meta;

public sealed record BandwidthPlugin(
    uint IngressRate,
    uint IngressBurst,
    uint EgressRate,
    uint EgressBurst,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("bandwidth", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["ingressRate"] = IngressRate;
        jsonObject["ingressBurst"] = IngressBurst;
        jsonObject["egressRate"] = EgressRate;
        jsonObject["egressBurst"] = EgressBurst;
    }
}