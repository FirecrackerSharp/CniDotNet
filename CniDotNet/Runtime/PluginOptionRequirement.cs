namespace CniDotNet.Runtime;

[Flags]
internal enum PluginOptionRequirement
{
    ContainerId = 1,
    InterfaceName = 2,
    NetworkNamespace = 3,
    Path = 4
}