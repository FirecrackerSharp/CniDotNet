using System.Text.Json.Nodes;

namespace CniDotNet.Typing.Main;

public sealed record TapPlugin(
    string? Mac = null,
    uint? Mtu = null,
    string? SeLinuxContext = null,
    bool? MultiQueue = null,
    uint? OwnerUid = null,
    uint? GroupGid = null,
    string? Bridge = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null) : TypedPlugin("tap", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (Mac is not null)
        {
            jsonObject["mac"] = Mac;
        }

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        if (SeLinuxContext is not null)
        {
            jsonObject["selinuxcontext"] = SeLinuxContext;
        }

        if (MultiQueue is not null)
        {
            jsonObject["multiQueue"] = MultiQueue;
        }

        if (OwnerUid is not null)
        {
            jsonObject["owner"] = OwnerUid;
        }

        if (GroupGid is not null)
        {
            jsonObject["group"] = GroupGid;
        }

        if (Bridge is not null)
        {
            jsonObject["bridge"] = Bridge;
        }
    }
}