using System.Text.Json.Serialization;

namespace CniDotNet.Data.Results.Add;

public sealed record AddCniResultDns(
    [property: JsonPropertyName("nameservers")] string[] Nameservers,
    [property: JsonPropertyName("search")] string[] Search,
    [property: JsonPropertyName("options")] string[] Options,
    [property: JsonPropertyName("domain")] string? Domain = null);