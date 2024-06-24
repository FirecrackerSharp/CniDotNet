using System.Text.Json.Nodes;
using CniDotNet.Data;

namespace CniDotNet.Typing;

public abstract record TypedNetwork(
    string Type,
    JsonObject? Capabilities)
{
    protected abstract void SerializePluginParameters(JsonObject jsonObject);

    public Network Build()
    {
        var jsonObject = new JsonObject();
        SerializePluginParameters(jsonObject);
        return new Network(Type, Capabilities, jsonObject);
    }
}