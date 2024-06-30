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
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);

        if (Master is not null)
        {
            jsonObject["master"] = Master;
        }

        jsonObject["mode"] = JsonSerializer.SerializeToNode(Mode, CniRuntime.SerializerOptions);

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