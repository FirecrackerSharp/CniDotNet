using CniDotNet.Abstractions;

namespace CniDotNet.Data;

public sealed record InvocationOptions(
    IRuntimeHost RuntimeHost,
    string? ElevationPassword = null,
    string SuPath = "/bin/su",
    string BashPath = "/bin/bash");