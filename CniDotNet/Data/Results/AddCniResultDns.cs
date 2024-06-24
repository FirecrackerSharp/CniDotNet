using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public sealed record AddCniResultDns(
    [property: JsonPropertyName("nameservers")] IReadOnlyList<string> Nameservers,
    [property: JsonPropertyName("search")] IReadOnlyList<string> Search,
    [property: JsonPropertyName("options")] IReadOnlyList<string> Options,
    [property: JsonPropertyName("domain")] string? Domain = null);