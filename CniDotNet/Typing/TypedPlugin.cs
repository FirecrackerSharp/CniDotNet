using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet.Typing;

public abstract record TypedPlugin(
    string Type,
    JsonObject? Capabilities)
{
    protected abstract void SerializePluginParameters(JsonObject jsonObject);

    public Plugin Build()
    {
        var jsonObject = new JsonObject();
        SerializePluginParameters(jsonObject);
        return new Plugin(Type, Capabilities, jsonObject);
    }
}