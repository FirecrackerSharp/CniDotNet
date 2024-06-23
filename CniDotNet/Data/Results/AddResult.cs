using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public record AddResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("ips")] IEnumerable<AddResultIp> Ips,
    [property: JsonPropertyName("dns")] AddResultDns Dns,
    [property: JsonPropertyName("interfaces")] IEnumerable<AddResultInterface> Interfaces,
    [property: JsonPropertyName("routes")] IEnumerable<AddResultRoute> Routes);