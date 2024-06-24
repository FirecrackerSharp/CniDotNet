namespace CniDotNet.Data;

public sealed record RuntimeOptions(
    string ContainerId,
    string NetworkNamespace,
    string InterfaceName,
    InvocationOptions InvocationOptions,
    PluginSearchOptions PluginSearchOptions,
    string? CniVersion = null)
{
    public static RuntimeOptions FromNetworkList(PluginList pluginList, string containerId, string networkNamespace,
        string interfaceName, InvocationOptions invocationOptions, PluginSearchOptions pluginSearchOptions)
    {
        return new RuntimeOptions(containerId, networkNamespace, interfaceName, invocationOptions,
            pluginSearchOptions, pluginList.CniVersion);
    }
}
