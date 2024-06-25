using CniDotNet.Data;

namespace CniDotNet.Typing;

public sealed record TypedPluginList(
    Version CniVersion,
    string Name,
    IReadOnlyList<TypedPlugin> Plugins,
    IEnumerable<Version>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false)
{
    public PluginList Build()
    {
        var builtPlugins = Plugins.Select(p => p.Build()).ToList();
        return new PluginList(
            CniVersion.ToString(),
            Name,
            builtPlugins,
            CniVersions?.Select(v => v.ToString()),
            DisableCheck,
            DisableGc);
    }
}