using System.Text.Json.Nodes;

namespace CniDotNet.Typing;

public sealed record TcRedirectTapPlugin(
    JsonObject? Capabilities = null,
    JsonObject? Args = null) : TypedPlugin("tc-redirect-tap", Capabilities, Args)
{
    protected override void SerializePluginParameters(JsonObject jsonObject) {}
}