using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;

namespace CniDotNet.Typing.Meta;

public sealed record TuningPlugin(
    string? DataDir = null,
    string? Mac = null,
    uint? Mtu = null,
    uint? TxQLen = null,
    bool? PromiscuousMode = null,
    bool? AllMulticastMode = null,
    Dictionary<string, string>? Sysctl = null,
    JsonObject? Capabilities = null,
    JsonObject? Args = null) : TypedPlugin("tuning", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject)
    {
        if (DataDir is not null)
        {
            jsonObject["dataDir"] = DataDir;
        }

        if (Mac is not null)
        {
            jsonObject["mac"] = Mac;
        }

        if (Mtu is not null)
        {
            jsonObject["mtu"] = Mtu;
        }

        if (TxQLen is not null)
        {
            jsonObject["txQLen"] = TxQLen;
        }

        if (PromiscuousMode is not null)
        {
            jsonObject["promisc"] = PromiscuousMode;
        }

        if (AllMulticastMode is not null)
        {
            jsonObject["allmulti"] = AllMulticastMode;
        }

        if (Sysctl is not null)
        {
            jsonObject["sysctl"] = JsonSerializer.SerializeToNode(Sysctl, CniRuntime.SerializerOptions);
        }
    }
}