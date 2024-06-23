using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results.Add;

public sealed record AddCniResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("ips")] IEnumerable<AddCniResultIp> Ips,
    [property: JsonPropertyName("dns")] AddCniResultDns Dns,
    [property: JsonPropertyName("interfaces")] IEnumerable<AddCniResultInterface> Interfaces,
    [property: JsonPropertyName("routes")] IEnumerable<AddCniResultRoute>? Routes = null);