using System.Text.Json.Nodes;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Meta;

public sealed record FirewallPlugin(
    FirewallBackend? Backend = null,
    string? IptablesAdminChainName = null,
    string? FirewalldZone = null,
    string? IngressPolicy = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null)
    : TypedPlugin("firewall", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
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