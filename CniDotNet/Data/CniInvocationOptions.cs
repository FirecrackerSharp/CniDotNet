namespace CniDotNet.Data;

public sealed record CniInvocationOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    InvocationOptions InvocationOptions,
    string? CniVersion = null,
    string? PluginPath = null)
{
    public static CniInvocationOptions FromNetworkList(NetworkList networkList, string containerId, string networkNamespace,
        InvocationOptions invocationOptions, string interfaceName, string? pluginPath = null)
    {
        return new CniInvocationOptions(
            containerId, networkNamespace, interfaceName, invocationOptions, networkList.CniVersion, pluginPath);
    }
}
