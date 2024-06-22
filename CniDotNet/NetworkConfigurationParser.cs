using System.Text.Json;
using System.Text.Json.Nodes;
using CniDotNet.Abstractions;
using CniDotNet.Data;

namespace CniDotNet;

public static class NetworkConfigurationParser
{
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