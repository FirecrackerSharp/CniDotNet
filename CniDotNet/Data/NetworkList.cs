namespace CniDotNet.Data;

public sealed record NetworkList(
    string CniVersion,
    string Name,
    IReadOnlyList<Network> Plugins,
    IEnumerable<string>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false);