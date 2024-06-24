using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CniDotNet.Data;

namespace CniDotNet.Typing;

public abstract record TypedNetwork(
    string Type,
    JsonObject? Capabilities)
{
    protected static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    protected abstract void SerializePluginParameters(JsonObject jsonObject);

    public Network Build()
    {
        var jsonObject = new JsonObject();
        SerializePluginParameters(jsonObject);
        return new Network(Type, Capabilities, jsonObject);
    }
}