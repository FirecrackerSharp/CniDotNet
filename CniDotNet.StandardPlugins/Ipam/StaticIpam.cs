using System.Text.Json.Serialization;
using CniDotNet.Data.Results;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record StaticIpam(
    [property: JsonPropertyName("type")] string Type = "static",
    [property: JsonPropertyName("addresses")] IReadOnlyList<StaticIpamAddress>? Addresses = null,
    [property: JsonPropertyName("routes")] IReadOnlyList<GenericIpamRoute>? Routes = null,
    [property: JsonPropertyName("dns")] AddCniResultDns? Dns = null);