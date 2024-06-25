using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public record TypedArgLabel(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value);