namespace CniDotNet.Data;

public sealed record PluginSearchOptions(
    string? Directory = null,
    string EnvironmentVariable = "PLUGIN_PATH",
    SearchOption DirectorySearchOption = SearchOption.TopDirectoryOnly)
{
    internal string? ActualDirectory => Directory ?? Environment.GetEnvironmentVariable(EnvironmentVariable);
}