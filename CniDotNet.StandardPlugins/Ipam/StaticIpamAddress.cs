using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record StaticIpamAddress(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("gateway")] string? Gateway = null);