using CniDotNet.Abstractions;

namespace CniDotNet.Data;

public sealed record RuntimeOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    ICniHost CniHost,
    string? ElevationPassword = null,
    string? CniVersion = null,
    string? PluginPath = null);
