using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record DhcpIpam(
    [property: JsonPropertyName("daemonSocketPath")] string? DaemonSocketPath = null,
    [property: JsonPropertyName("request")] DhcpRequestOptions? RequestOptions = null,
    [property: JsonPropertyName("provide")] DhcpLeaseOptions? LeaseOptions = null);