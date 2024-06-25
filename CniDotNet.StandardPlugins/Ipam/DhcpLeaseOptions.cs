using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record DhcpLeaseOptions(
    [property: JsonPropertyName("option")] string? Option = null,
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("fromArg")] string? FromArgument = null);