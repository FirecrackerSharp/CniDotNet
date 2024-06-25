using System.Text.Json.Serialization;

namespace CniDotNet.Typing;

public sealed record TypedCapabilityBandwidth(
    [property: JsonPropertyName("ingressRate")] uint IngressRate,
    [property: JsonPropertyName("ingressBurst")] uint IngressBurst,
    [property: JsonPropertyName("egressRate")] uint EgressRate,
    [property: JsonPropertyName("egressBurst")] uint EgressBurst);