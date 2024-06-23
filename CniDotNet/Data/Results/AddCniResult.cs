using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public sealed record AddCniResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("ips")] IEnumerable<AddCniResultIp> Ips,
    [property: JsonPropertyName("dns")] AddCniResultDns Dns,
    [property: JsonPropertyName("interfaces")] IEnumerable<AddCniResultInterface> Interfaces,
    [property: JsonPropertyName("routes")] IEnumerable<AddCniResultRoute>? Routes = null);