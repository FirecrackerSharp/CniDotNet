using System.Text.Json.Serialization;

namespace CniDotNet.Data.CniResults;

public sealed record ErrorCniResult(
    [property: JsonPropertyName("code")] uint Code,
    [property: JsonPropertyName("msg")] string Message,
    [property: JsonPropertyName("details")]
    string? Details = null,
    [property: JsonPropertyName("cniVersion")]
    string? CniVersion = null)
{
    public WellKnownError? Error
    {
        get
        {
            if (!Enum.IsDefined(typeof(WellKnownError), (int)Code)) return null;
            return (WellKnownError) (int)Code;
        }
    }
}