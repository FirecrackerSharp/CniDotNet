using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record HostLocalIpamRoute(
    [property: JsonPropertyName("dst")] string Destination,
    [property: JsonPropertyName("gw")] string? Gateway = null);