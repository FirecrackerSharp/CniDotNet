using System.Text.Json.Nodes;
using CniDotNet.Typing;

namespace CniDotNet.StandardPlugins.Main;

public sealed record TcRedirectTapPlugin(
    TypedCapabilities? Capabilities = null,
    TypedArgs? Args = null) : TypedPlugin("tc-redirect-tap", Capabilities, Args)
{
    public override void SerializePluginParameters(JsonObject jsonObject) {}
}