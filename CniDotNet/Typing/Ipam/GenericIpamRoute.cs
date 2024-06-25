using System.Text.Json.Serialization;

namespace CniDotNet.Typing.Ipam;

public sealed record GenericIpamRoute(
    [property: JsonPropertyName("dst")] string Destination,
    [property: JsonPropertyName("gw")] string? Gateway = null);