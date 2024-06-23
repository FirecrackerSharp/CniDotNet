namespace CniDotNet.Data;

public sealed record NetworkConfiguration(
    Version CniVersion,
    string Name,
    IReadOnlyList<NetworkPlugin> Plugins,
    IEnumerable<Version>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false);