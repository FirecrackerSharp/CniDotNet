using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data;
using CniDotNet.Host;

namespace CniDotNet;

public static class NetworkConfigurationLoader
{
    public static async Task<NetworkConfiguration?> LookupFirstAsync(
        ICniHost cniHost, ConfigurationLookupOptions configurationLookupOptions, CancellationToken cancellationToken = default)
    {
        var matches = await LookupManyAsync(cniHost, configurationLookupOptions, cancellationToken);
        return matches.Count == 0 ? null : matches[0];
    }
    
    public static async Task<IReadOnlyList<NetworkConfiguration>> LookupManyAsync(
        ICniHost cniHost, ConfigurationLookupOptions configurationLookupOptions, CancellationToken cancellationToken = default)
    {
        var directory = configurationLookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(configurationLookupOptions.EnvironmentVariable);
        if (directory is null) return [];

        if (!cniHost.DirectoryExists(directory)) return [];

        var files = cniHost
            .EnumerateDirectory(directory, configurationLookupOptions.SearchQuery ?? "", configurationLookupOptions.DirectorySearchOption)
            .Where(f => configurationLookupOptions.FileExtensions.Contains(Path.GetExtension(f)));

        var configurations = new List<NetworkConfiguration>();

        foreach (var file in files)
        {
            try
            {
                var configuration = await LoadFromFileAsync(cniHost, file, cancellationToken);
                configurations.Add(configuration);
            }
            catch (Exception)
            {
                if (configurationLookupOptions.ProceedAfterFailure) continue;
                return [];
            }
        }

        return configurations;
    }
    
    public static async Task<NetworkConfiguration> LoadFromFileAsync(
        ICniHost cniHost,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var sourceString = await cniHost.ReadFileAsync(filePath, cancellationToken);
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
        var cniVersion = jsonNode[Constants.Parsing.CniVersion]!.GetValue<string>();

        IEnumerable<string>? cniVersions = null;
        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.CniVersions))
        {
            cniVersions = jsonNode[Constants.Parsing.CniVersions]!.AsArray().GetValues<string>();
        }

        var name = jsonNode[Constants.Parsing.Name]!.GetValue<string>();
        var disableCheck = false;

        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.DisableCheck))
        {
            disableCheck = jsonNode[Constants.Parsing.DisableCheck]!.GetValue<bool>();
        }

        var disableGc = false;
        if (jsonNode.AsObject().ContainsKey(Constants.Parsing.DisableGc))
        {
            disableGc = jsonNode[Constants.Parsing.DisableGc]!.GetValue<bool>();
        }

        var plugins = jsonNode[Constants.Parsing.Plugins]!.AsArray()
            .Select(pluginJsonNode => LoadPlugin(pluginJsonNode!))
            .ToList();

        return new NetworkConfiguration(
            cniVersion, name, plugins, cniVersions, disableCheck, disableGc);
    }

    private static NetworkPlugin LoadPlugin(JsonNode jsonNode)
    {
        var type = jsonNode[Constants.Parsing.Type]!.GetValue<string>();
        var capabilities = jsonNode[Constants.Parsing.Capabilities]?.AsObject();

        var pluginParameters = jsonNode.AsObject();
        pluginParameters.Remove(Constants.Parsing.Type);
        if (capabilities is not null) pluginParameters.Remove(Constants.Parsing.Capabilities);

        return new NetworkPlugin(type, capabilities, pluginParameters);
    }
}