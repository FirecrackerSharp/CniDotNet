using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record MacvlanPlugin(
    object Ipam,
    string? Master = null,
    MacvlanMode Mode = MacvlanMode.Bridge,
    uint? Mtu = null,
    bool? LinkInContainer = null,
    string? Mac = null,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("macvlan", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);

        if (Master is not null)
        {
            jsonObject["master"] = Master;
        }

        jsonObject["mode"] = Mode switch
        {
            MacvlanMode.Bridge => "bridge",
            MacvlanMode.Private => "private",
            MacvlanMode.Vepa => "vepa",
            MacvlanMode.Passthru => "passthru",
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

        if (Mac is not null)
        {
            jsonObject["mac"] = Mac;
        }
    }
}