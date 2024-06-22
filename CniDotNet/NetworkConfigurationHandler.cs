using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkConfigurationHandler
{
    public static async Task<NetworkConfiguration?> LookupFirstAsync(
        IFilesystem filesystem, LookupOptions lookupOptions, CancellationToken cancellationToken = default)
    {
        var matches = await LookupManyAsync(filesystem, lookupOptions, cancellationToken);
        return matches.Count == 0 ? null : matches[0];
    }
    
    public static async Task<IReadOnlyList<NetworkConfiguration>> LookupManyAsync(
        IFilesystem filesystem, LookupOptions lookupOptions, CancellationToken cancellationToken = default)
    {
        var directory = lookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(lookupOptions.EnvironmentVariable);
        if (directory is null) return [];

        if (!Directory.Exists(directory)) return [];

        var files = Directory
            .EnumerateFiles(directory, lookupOptions.SearchQuery ?? "", lookupOptions.DirectorySearchOption)
            .Where(f => lookupOptions.FileExtensions.Contains(Path.GetExtension(f)));

        var configurations = new List<NetworkConfiguration>();

        foreach (var file in files)
        {
            try
            {
                var configuration = await LoadFromFileAsync(filesystem, file, cancellationToken);
                configurations.Add(configuration);
            }
            catch (Exception)
            {
                if (lookupOptions.ProceedAfterFailure) continue;
                return [];
            }
        }

        return configurations;
    }
    
    public static async Task<NetworkConfiguration> LoadFromFileAsync(
        IFilesystem filesystem,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var sourceString = await filesystem.ReadFileAsync(filePath, cancellationToken);
        return LoadFromString(sourceString);
    }

    public static NetworkConfiguration LoadFromString(string sourceString)
    {
        var jsonNode = JsonSerializer.Deserialize<JsonNode>(sourceString)!;
        var configuration = LoadConfiguration(jsonNode);

        return configuration;
    }

    private static NetworkConfiguration LoadConfiguration(JsonNode jsonNode)
    {
        var cniVersion = Version.Parse(jsonNode[ParsingConstants.CniVersion]!.GetValue<string>());

        IEnumerable<Version>? cniVersions = null;
        if (jsonNode.AsObject().ContainsKey(ParsingConstants.CniVersions))
        {
            cniVersions = jsonNode[ParsingConstants.CniVersions]!
                .AsArray().GetValues<string>()
                .Select(Version.Parse);
        }

        var name = jsonNode[ParsingConstants.Name]!.GetValue<string>();
        var disableCheck = false;

        if (jsonNode.AsObject().ContainsKey(ParsingConstants.DisableCheck))
        {
            disableCheck = jsonNode[ParsingConstants.DisableCheck]!.GetValue<bool>();
        }

        var disableGc = false;
        if (jsonNode.AsObject().ContainsKey(ParsingConstants.DisableGc))
        {
            disableGc = jsonNode[ParsingConstants.DisableGc]!.GetValue<bool>();
        }

        var plugins = jsonNode[ParsingConstants.Plugins]!.AsArray()
            .Select(pluginJsonNode => LoadPlugin(pluginJsonNode!))
            .ToList();

        return new NetworkConfiguration(
            cniVersion, name, plugins, cniVersions, disableCheck, disableGc);
    }

    private static NetworkPlugin LoadPlugin(JsonNode jsonNode)
    {
        var type = jsonNode[ParsingConstants.Type]!.GetValue<string>();
        var capabilities = jsonNode[ParsingConstants.Capabilities]?.AsObject();

        var pluginParameters = jsonNode.AsObject();
        pluginParameters.Remove(ParsingConstants.Type);
        if (capabilities is not null) pluginParameters.Remove(ParsingConstants.Capabilities);

        return new NetworkPlugin(type, capabilities, pluginParameters);
    }
}