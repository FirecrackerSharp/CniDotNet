using System.Text.Json.Nodes;

namespace CniDotNet.Data;

public sealed record Plugin(
    string Type,
    JsonObject? Capabilities,
    JsonObject? Args,
    JsonObject PluginParameters);