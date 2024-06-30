using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Runtime;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record DummyPlugin(
    object Ipam,
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("dummy", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject)
    {
        jsonObject["ipam"] = JsonSerializer.SerializeToNode(Ipam, CniRuntime.SerializerOptions);
    }
}