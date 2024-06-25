using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record TypedCapabilityPortMapping(
    [property: JsonPropertyName("hostPort")] uint HostPort,
    [property: JsonPropertyName("containerPort")] uint ContainerPort,
    [property: JsonPropertyName("protocol")] TypedCapabilityPortProtocol Protocol = TypedCapabilityPortProtocol.Tcp);