using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Data;
using CniDotNet.Host;

namespace CniDotNet.Runtime;

public static class NetworkLists
{
    public static async Task<NetworkList?> LookupFirstAsync(
        ICniHost cniHost, NetworkListLookupOptions networkListLookupOptions, CancellationToken cancellationToken = default)
    {
        var matches = await LookupManyAsync(cniHost, networkListLookupOptions, cancellationToken);
        return matches.Count == 0 ? null : matches[0];
    }
    
    public static async Task<IReadOnlyList<NetworkList>> LookupManyAsync(
        ICniHost cniHost, NetworkListLookupOptions networkListLookupOptions, CancellationToken cancellationToken = default)
    {
        var directory = networkListLookupOptions.Directory ??
                        Environment.GetEnvironmentVariable(networkListLookupOptions.EnvironmentVariable);
        if (directory is null) return [];

        if (!cniHost.DirectoryExists(directory)) return [];

        var files = cniHost
            .EnumerateDirectory(directory, networkListLookupOptions.SearchQuery ?? "", networkListLookupOptions.DirectorySearchOption)
            .Where(f => networkListLookupOptions.FileExtensions.Contains(Path.GetExtension(f)));

        var networkLists = new List<NetworkList>();

        foreach (var file in files)
        {
            try
            {
                var configuration = await LoadFromFileAsync(cniHost, file, cancellationToken);
                networkLists.Add(configuration);
            }
            catch (Exception)
            {
                if (networkListLookupOptions.ProceedAfterFailure) continue;
                return [];
            }
        }

        return networkLists;
    }
    
    public static async Task<NetworkList> LoadFromFileAsync(
        ICniHost cniHost,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var sourceString = await cniHost.ReadFileAsync(filePath, cancellationToken);
        return LoadFromString(sourceString);
    }

    public static NetworkList LoadFromString(string sourceString)
    {
        var jsonNode = JsonSerializer.Deserialize<JsonNode>(sourceString)!;
        var configuration = LoadConfiguration(jsonNode);

        return configuration;
    }

    private static NetworkList LoadConfiguration(JsonNode jsonNode)
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

        return new NetworkList(
            cniVersion, name, plugins, cniVersions, disableCheck, disableGc);
    }

    private static Network LoadPlugin(JsonNode jsonNode)
    {
        var type = jsonNode[Constants.Parsing.Type]!.GetValue<string>();
        var capabilities = jsonNode[Constants.Parsing.Capabilities]?.AsObject();

        var pluginParameters = jsonNode.AsObject();
        pluginParameters.Remove(Constants.Parsing.Type);
        if (capabilities is not null) pluginParameters.Remove(Constants.Parsing.Capabilities);

        return new Network(type, capabilities, pluginParameters);
    }
}