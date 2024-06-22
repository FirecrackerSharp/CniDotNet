namespace CniDotNet.Data;

public sealed record LookupOptions(
    string[] FileExtensions,
    string? Directory = null,
    string? SearchQuery = null,
    string EnvironmentVariable = "CONF_LIST_PATH",
    SearchOption DirectorySearchOption = SearchOption.TopDirectoryOnly,
    bool ProceedAfterFailure = true);
