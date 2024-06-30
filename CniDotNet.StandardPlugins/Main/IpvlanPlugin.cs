using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record IpvlanPlugin(
    object? Ipam = null,
    string? Master = null,
    IpvlanMode Mode = IpvlanMode.L2,
    uint? Mtu = null,
    bool? LinkInContainer = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("ipvlan", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (Ipam is not null)
        {
            jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);
        }

        if (Master is not null)
        {
            jsonObject["master"] = Master;
        }

        jsonObject["mode"] = Mode switch
        {
            IpvlanMode.L2 => "l2",
            IpvlanMode.L3 => "l3",
            IpvlanMode.L3S => "l3s",
            _ => throw new ArgumentOutOfRangeException(nameof(jsonObject))
        };

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        if (LinkInContainer is not null)
        {
            jsonObject["linkInContainer"] = LinkInContainer;
        }
    }
}