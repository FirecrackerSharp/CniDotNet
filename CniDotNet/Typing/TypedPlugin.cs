using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet.Typing;

public abstract record TypedPlugin(
    string Type,
    TypedCapabilities? Capabilities,
    TypedArgs? Args)
{
    public abstract void SerializePluginParameters(JsonObject jsonObject);

    public Plugin Build()
    {
        var jsonObject = new JsonObject();
        SerializePluginParameters(jsonObject);
        return new Plugin(Type, Capabilities?.Serialize(), Args?.Serialize(), jsonObject);
    }
}