using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Ipam;

public sealed record DhcpRequestOptions(
    [property: JsonPropertyName("skipDefault")] bool? SkipDefault = null,
    [property: JsonPropertyName("option")] string? Option = null);