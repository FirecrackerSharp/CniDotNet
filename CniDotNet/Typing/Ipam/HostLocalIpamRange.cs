using System.Text.Json.Serialization;

namespace CniDotNet.Typing.Ipam;

public sealed record HostLocalIpamRange(
    [property: JsonPropertyName("rangeStart")] string? RangeStart = null,
    [property: JsonPropertyName("rangeEnd")] string? RangeEnd = null,
    [property: JsonPropertyName("subnet")] string? Subnet = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);