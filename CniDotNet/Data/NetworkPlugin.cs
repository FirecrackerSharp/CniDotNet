using System.Text.Json.Nodes;

namespace CniDotNet.Data;

public sealed record NetworkPlugin(
    string Type,
    JsonObject? Capabilities,
    JsonObject PluginParameters)
{
    internal string OriginalJson { get; init; }
}
