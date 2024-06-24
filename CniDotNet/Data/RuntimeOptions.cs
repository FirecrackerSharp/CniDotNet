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
    string SuPath = "/bin/su")
{
    public static RuntimeOptions FromConfiguration(NetworkList networkList,
        string containerId, string networkNamespace, string interfaceName, ICniHost cniHost,
        string? elevationPassword = null, string? pluginPath = null, string suPath = "/bin/su")
    {
        return new RuntimeOptions(
            containerId, networkNamespace, interfaceName, cniHost, elevationPassword, networkList.CniVersion,
            pluginPath, suPath);
    }
}
