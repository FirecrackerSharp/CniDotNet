using System.Text.Json.Nodes;

namespace CniDotNet.Typing.Meta;

public sealed record FirewallPlugin(
    FirewallBackend? Backend = null,
    string? IptablesAdminChainName = null,
    string? FirewalldZone = null,
    string? IngressPolicy = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null)
    : TypedPlugin("firewall", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (Backend.HasValue)
        {
            jsonObject["backend"] = Backend == FirewallBackend.Firewalld ? "firewalld" : "iptables";
        }

        if (IptablesAdminChainName is not null)
        {
            jsonObject["iptablesAdminChainName"] = IptablesAdminChainName;
        }

        if (FirewalldZone is not null)
        {
            jsonObject["firewalldZone"] = FirewalldZone;
        }
        
        if (IngressPolicy is not null)
        {
            jsonObject["ingressPolicy"] = IngressPolicy;
        }
    }
}