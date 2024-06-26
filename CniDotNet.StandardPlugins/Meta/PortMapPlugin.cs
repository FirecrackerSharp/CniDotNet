using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Meta;

public sealed record PortMapPlugin(
    bool? Snat = null,
    bool? MasqueradeAll = null,
    ushort? MarkMasqueradeBit = null,
    string? ExternalSetMarkChain = null,
    IReadOnlyList<string>? ConditionsIpV4 = null,
    IReadOnlyList<string>? ConditionsIpV6 = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("portmap", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (Snat is not null)
        {
            jsonObject["snat"] = Snat;
        }

        if (MasqueradeAll is not null)
        {
            jsonObject["masqAll"] = MasqueradeAll;
        }

        if (MarkMasqueradeBit is not null)
        {
            jsonObject["markMasqBit"] = MarkMasqueradeBit;
        }

        if (ExternalSetMarkChain is not null)
        {
            jsonObject["externalSetMarkChain"] = ExternalSetMarkChain;
        }

        if (ConditionsIpV4 is not null)
        {
            jsonObject["conditionsV4"] = JsonSerializer.Serialize(ConditionsIpV4, CniRuntime.SerializerOptions);
        }

        if (ConditionsIpV6 is not null)
        {
            jsonObject["conditionsV6"] = JsonSerializer.Serialize(ConditionsIpV6, CniRuntime.SerializerOptions);
        }
    }
}