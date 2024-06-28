using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record CniAddResultIp(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("interface")] uint? Interface = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);