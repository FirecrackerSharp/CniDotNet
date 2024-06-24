using System.Text.Json.Nodes;

namespace CniDotNet.Typing;

public sealed record TcRedirectTapPlugin(
    JsonObject? Capabilities = null) : TypedPlugin("tc-redirect-tap", Capabilities)
{
    protected override void SerializePluginParameters(JsonObject jsonObject) {}
}