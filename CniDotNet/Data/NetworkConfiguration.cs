namespace CniDotNet.Data;

public sealed record NetworkConfiguration(
    string CniVersion,
    string Name,
    IReadOnlyList<NetworkPlugin> Plugins,
    IEnumerable<string>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false);