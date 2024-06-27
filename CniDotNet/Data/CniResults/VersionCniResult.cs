using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record VersionCniResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("supportedVersions")] IReadOnlyList<string> SupportedVersions);