namespace CniDotNet.Data;

public sealed record PluginLookupOptions(
    string? Directory = null,
    string EnvironmentVariable = "PLUGIN_PATH",
    SearchOption DirectorySearchOption = SearchOption.TopDirectoryOnly)
{
    public static readonly PluginLookupOptions Default = new();
}