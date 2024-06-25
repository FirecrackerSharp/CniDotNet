using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record TypedCapabilityDns(
    [property: JsonPropertyName("searches")] IReadOnlyList<string> Searches,
    [property: JsonPropertyName("servers")] IReadOnlyList<string> Servers);