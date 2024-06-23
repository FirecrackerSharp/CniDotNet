using CniDotNet.Host;

namespace CniDotNet.Data;

public record RuntimeOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    ICniHost CniHost,
    string? ElevationPassword = null,
    string? CniVersion = null,
    string? PluginPath = null,
    string SuPath = "/bin/su");
