using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record GenericIpamRoute(
    [property: JsonPropertyName("dst")] string Destination,
    [property: JsonPropertyName("gw")] string? Gateway = null);