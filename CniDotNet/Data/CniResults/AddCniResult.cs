using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record AddCniResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("ips")] IReadOnlyList<AddCniResultIp> Ips,
    [property: JsonPropertyName("dns")] AddCniResultDns Dns,
    [property: JsonPropertyName("interfaces")] IReadOnlyList<AddCniResultInterface> Interfaces,
    [property: JsonPropertyName("routes")] IReadOnlyList<AddCniResultRoute>? Routes = null);