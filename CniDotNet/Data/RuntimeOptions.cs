using CniDotNet.Host;

namespace CniDotNet.Data;

public record RuntimeOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    ICniHost CniHost,
    Dictionary<string, string> Arguments,
    string? ElevationPassword = null,
    string? CniVersion = null,
    string? PluginPath = null,
    string SuPath = "/bin/su")
{
    public static RuntimeOptions FromConfiguration(NetworkConfiguration networkConfiguration,
        string containerId, string networkNamespace, string interfaceName, ICniHost cniHost,
        Dictionary<string, string> arguments, string? elevationPassword = null, string? pluginPath = null, string suPath = "/bin/su")
    {
        return new RuntimeOptions(
            containerId, networkNamespace, interfaceName, cniHost, arguments, elevationPassword, networkConfiguration.CniVersion,
            pluginPath, suPath);
    }
}
