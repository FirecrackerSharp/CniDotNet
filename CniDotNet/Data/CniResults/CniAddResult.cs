using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record CniAddResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("ips")] IReadOnlyList<CniAddResultIp> Ips,
    [property: JsonPropertyName("dns")] CniAddResultDns Dns,
    [property: JsonPropertyName("interfaces")] IReadOnlyList<CniAddResultInterface> Interfaces,
    [property: JsonPropertyName("routes")] IReadOnlyList<CniAddResultRoute>? Routes = null);