namespace CniDotNet.Data;

public sealed record PluginSearchOptions(
    IReadOnlyDictionary<string, string>? SearchTable = null,
    string? Directory = null,
    string EnvironmentVariable = "PLUGIN_PATH",
    SearchOption DirectorySearchOption = SearchOption.TopDirectoryOnly)
{
    internal string? ActualDirectory => Directory ?? Environment.GetEnvironmentVariable(EnvironmentVariable);
}