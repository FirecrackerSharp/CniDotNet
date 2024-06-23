using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results;

public record AddResultDns(
    [property: JsonPropertyName("nameservers")] string[] Nameservers,
    [property: JsonPropertyName("search")] string[] Search,
    [property: JsonPropertyName("options")] string[] Options,
    [property: JsonPropertyName("domain")] string? Domain = null);