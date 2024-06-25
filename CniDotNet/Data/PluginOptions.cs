namespace CniDotNet.Data;

public sealed record PluginOptions(
    string CniVersion,
    string Name,
    string? ContainerId = null,
    string? NetworkNamespace = null,
    string? InterfaceName = null,
    bool IncludePath = true,
    bool SkipValidation = false)
{
    public static PluginOptions FromPluginList(PluginList pluginList, string? containerId = null,
        string? networkNamespace = null, string? interfaceName = null)
    {
        return new PluginOptions(pluginList.CniVersion, pluginList.Name, containerId, networkNamespace, interfaceName);
    }
}