using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results.Add;

public sealed record AddCniResultRoute(
    [property: JsonPropertyName("dst")] string Dst,
    [property: JsonPropertyName("gw")] string Gw,
    [property: JsonPropertyName("mtu")] uint Mtu,
    [property: JsonPropertyName("advmss")] uint Advmss,
    [property: JsonPropertyName("priority")] uint Priority,
    [property: JsonPropertyName("table")] uint Table,
    [property: JsonPropertyName("scope")] uint Scope);