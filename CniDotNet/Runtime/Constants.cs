namespace CniDotNet.Runtime;

internal static class Constants
{
    internal static class Parsing
    {
        public const string Name = "name";
        public const string CniVersions = "cniVersions";
        public const string CniVersion = "cniVersion";
        public const string DisableCheck = "disableCheck";
        public const string DisableGc = "disableGC";
        public const string Plugins = "plugins";
        public const string Type = "type";
        public const string Capabilities = "capabilities";
        public const string Args = "args";
        public const string RuntimeConfig = "runtimeConfig";
        public const string PreviousResult = "prevResult";
    }

    internal static class Environment
    {
        public const string Command = "CNI_COMMAND";
        public const string ContainerId = "CNI_CONTAINERID";
        public const string NetworkNamespace = "CNI_NETNS";
        public const string InterfaceName = "CNI_IFNAME";
        public const string PluginPath = "CNI_PATH";
    }

    internal static class Operations
    {
        public const string Add = "ADD";
        public const string Delete = "DEL";
        public const string Check = "CHECK";
        public const string Status = "STATUS";
        public const string ProbeVersions = "VERSION";
        public const string GarbageCollect = "GC";
    }
}