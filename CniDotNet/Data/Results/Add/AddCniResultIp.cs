using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results.Add;

public sealed record AddCniResultIp(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("interface")] uint? Interface = null,
    [property: JsonPropertyName("gateway")] string? Gateway = null);