using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public sealed record AddCniResultIp(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("interface")] uint? Interface = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);