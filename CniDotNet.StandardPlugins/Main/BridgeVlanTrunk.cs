using System.Text.Json.Serialization;

namespace CniDotNet.StandardPlugins.Main;

public sealed record BridgeVlanTrunk(
    [property: JsonPropertyName("minID")] int? MinId = null,
    [property: JsonPropertyName("maxID")] int? MaxId = null,
    [property: JsonPropertyName("ID")] int? Id = null);