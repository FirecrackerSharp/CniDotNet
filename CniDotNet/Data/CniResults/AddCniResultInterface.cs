using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record AddCniResultInterface(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("mac")] string? Mac = null,
    [property: JsonPropertyName("mtu")] uint? Mtu = null,
    [property: JsonPropertyName("sandbox")] string? Sandbox = null,
    [property: JsonPropertyName("socketPath")] string? SocketPath = null,
    [property: JsonPropertyName("pciID")] string? PciId = null);