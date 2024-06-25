using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record HostLocalIpam(
    [property: JsonPropertyName("ranges")] HostLocalIpamRange[][] Ranges,
    [property: JsonPropertyName("type")] string Type = "host-local",
    [property: JsonPropertyName("resolvConf")] string? ResolvConf = null,
    [property: JsonPropertyName("dataDir")] string? DataDir = null,
    [property: JsonPropertyName("routes")] IReadOnlyList<GenericIpamRoute>? Routes = null);