using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record TypedCapabilityIpRange(
    [property: JsonPropertyName("subnet")] string? Subnet = null,
    [property: JsonPropertyName("rangeStart")] string? RangeStart = null,
    [property: JsonPropertyName("rangeEnd")] string? RangeEnd = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);