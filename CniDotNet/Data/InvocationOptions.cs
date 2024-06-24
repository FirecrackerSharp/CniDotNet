using CniDotNet.Host;

namespace CniDotNet.Data;

public sealed record InvocationOptions(
    ICniHost CniHost,
    string? ElevationPassword = null,
    string SuPath = "/bin/su",
    string BashPath = "/bin/bash");