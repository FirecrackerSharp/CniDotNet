namespace CniDotNet.Data;

public sealed record RuntimeOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    string? PluginPath = null);
