using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public sealed record VersionCniResult(
    [property: JsonPropertyName("cniVersion")] string CniVersion,
    [property: JsonPropertyName("supportedVersions")] string[] SupportedVersions);