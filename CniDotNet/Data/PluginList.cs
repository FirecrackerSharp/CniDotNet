namespace CniDotNet.Data;

public sealed record PluginList(
    string CniVersion,
    string Name,
    IReadOnlyList<Plugin> Plugins,
    IEnumerable<string>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false);