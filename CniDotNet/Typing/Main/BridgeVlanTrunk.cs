using System.Text.Json.Serialization;

namespace CniDotNet.Typing.Main;

public sealed record BridgeVlanTrunk(
    [property: JsonPropertyName("minID")] int? MinId = null,
    [property: JsonPropertyName("maxID")] int? MaxId = null,
    [property: JsonPropertyName("ID")] int? Id = null);