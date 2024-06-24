using CniDotNet.Data;

namespace CniDotNet.Typing;

public sealed record TypedPluginList(
    Version CniVersion,
    string Name,
    IReadOnlyList<TypedPlugin> Networks,
    IEnumerable<Version>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false)
{
    public PluginList Build()
    {
        var builtNetworks = Networks.Select(p => p.Build()).ToList();
        return new PluginList(
            CniVersion.ToString(),
            Name,
            builtNetworks,
            CniVersions?.Select(v => v.ToString()),
            DisableCheck,
            DisableGc);
    }
}