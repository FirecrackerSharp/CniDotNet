using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public record AddResultIp(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("interface")] uint? Interface = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);