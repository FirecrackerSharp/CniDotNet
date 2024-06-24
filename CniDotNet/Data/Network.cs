using System.Text.Json.Nodes;

namespace CniDotNet.Data;

public sealed record Network(
    string Type,
    JsonObject? Capabilities,
    JsonObject PluginParameters);