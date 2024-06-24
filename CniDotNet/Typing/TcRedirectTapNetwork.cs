using System.Text.Json.Nodes;

namespace CniDotNet.Typing;

public record TcRedirectTapNetwork(
    JsonObject? Capabilities = null) : TypedNetwork("tc-redirect-tap", Capabilities)
{
    protected override void SerializePluginParameters(JsonObject jsonObject) {}
}