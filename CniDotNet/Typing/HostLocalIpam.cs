using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record HostLocalIpam(
    [property: JsonPropertyName("ranges")] IReadOnlyList<HostLocalIpamRange> Ranges,
    [property: JsonPropertyName("type")] string Type = "host-local",
    [property: JsonPropertyName("resolvConf")] string? ResolvConf = null,
    [property: JsonPropertyName("dataDir")] string? DataDir = null,
    [property: JsonPropertyName("routes")] IReadOnlyList<HostLocalIpamRoute>? Routes = null);