using CniDotNet.Data;

namespace CniDotNet.Typing;

public sealed record TypedNetworkList(
    Version CniVersion,
    string Name,
    IReadOnlyList<TypedNetwork> Networks,
    IEnumerable<Version>? CniVersions = null,
    bool DisableCheck = false,
    bool DisableGc = false)
{
    public NetworkList Build()
    {
        var builtNetworks = Networks.Select(n => n.Build()).ToList();
        return new NetworkList(
            CniVersion.ToString(),
            Name,
            builtNetworks,
            CniVersions?.Select(v => v.ToString()),
            DisableCheck,
            DisableGc);
    }
}